using System.Net;
using System.Text.Json;
using LocationGuesser.Blazor.Services;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using RichardSzalay.MockHttp;
using static LocationGuesser.Blazor.Tests.Helpers.JsonHelper;

namespace LocationGuesser.Blazor.Tests.Services;

public class GameApiServiceTests
{
    private readonly GameApiService _cut;

    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly MockHttpMessageHandler _mockHttp = new();

    public GameApiServiceTests()
    {
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost:5000");
        _cut = new GameApiService(client);
    }

    [Fact]
    public async Task GetGameSetAsync_ShouldReturnFail_WhenHttpCallFails()
    {
        var setId = Guid.NewGuid();
        var imageCount = 5;
        _mockHttp.When($"/api/games/{setId}")
            .Respond(HttpStatusCode.InternalServerError);

        var result = await _cut.GetGameSetAsync(setId, imageCount, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetGameSetAsync_ShouldReturnNotFound_WhenHttpCallFailsWith404()
    {
        var setId = Guid.NewGuid();
        var imageCount = 5;
        _mockHttp.When($"/api/games/{setId}")
            .Respond(HttpStatusCode.NotFound);

        var result = await _cut.GetGameSetAsync(setId, imageCount, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task GetGameSetAsync_ShouldReturnListOfImages_WhenHttpCallReturnsWith200()
    {
        var setId = Guid.NewGuid();
        var imageCount = 5;
        var images = CreateImages(setId);
        _mockHttp.When($"/api/games/{setId}")
            .Respond(HttpStatusCode.OK, "application/json", ToJson(images));

        var result = await _cut.GetGameSetAsync(setId, imageCount, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(images);
    }

    [Fact]
    public async Task GetGameSetAsync_ShouldReturnFail_WhenJsonIsInvalid()
    {
        var setId = Guid.NewGuid();
        var imageCount = 5;
        _mockHttp.When($"/api/games/{setId}")
            .Respond(HttpStatusCode.OK, "application/json", "Invalid json");

        var result = await _cut.GetGameSetAsync(setId, imageCount, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetGameSetAsync_ShouldPassImageCountViaQuery()
    {
        var setId = Guid.NewGuid();
        var imageCount = 5;
        var images = CreateImages(setId);
        _mockHttp.When($"/api/games/{setId}")
            .WithQueryString("imageCount", imageCount.ToString())
            .Respond(HttpStatusCode.OK, "application/json", ToJson(images));

        var result = await _cut.GetGameSetAsync(setId, imageCount, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void GetImageContentUrl_ShouldReturnCorrectUrl()
    {
        var setId = Guid.NewGuid();
        var imageId = 1;
        var expectedUrl = $"/api/games/{setId}/image/1/content";

        var result = _cut.GetImageContentUrl(setId, imageId);

        result.Should().Be(expectedUrl);
    }

    private List<Image> CreateImages(Guid setId, int count = 5)
    {
        return Enumerable.Range(1, count)
            .Select(x => new Image(setId, x, 2023, 10, 20, $"Description {x}", ""))
            .ToList();
    }
}