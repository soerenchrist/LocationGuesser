using System.Net;
using System.Text.Json;
using LocationGuesser.Blazor.Services;
using LocationGuesser.Core.Contracts;
using LocationGuesser.Core.Domain.Errors;
using RichardSzalay.MockHttp;

namespace LocationGuesser.Blazor.Tests.Services;

public class ImageSetApiServiceTests
{
    private readonly ImageSetApiService _cut;
    private MockHttpMessageHandler _mockHttp = new();
    private JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ImageSetApiServiceTests()
    {
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost:5000");
        _cut = new ImageSetApiService(client);
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnFail_WhenHttpCallReturnsInternalServerError()
    {
        _mockHttp.When("/api/imagesets")
            .Respond(HttpStatusCode.InternalServerError, "application/json", CreateErrorJson());

        var result = await _cut.ListImageSetsAsync(default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnNotFound_WhenHttpCallReturnsNotFound()
    {
        _mockHttp.When("/api/imagesets")
            .Respond(HttpStatusCode.NotFound, "application/json", CreateErrorJson());

        var result = await _cut.ListImageSetsAsync(default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnEmptyList_WhenHttpCallReturnsEmptyList()
    {
        _mockHttp.When("/api/imagesets")
            .Respond(HttpStatusCode.OK, "application/json", "[]");

        var result = await _cut.ListImageSetsAsync(default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnListOfImageSets_WhenHttpCallReturnsListOfImageSets()
    {
        var contracts = CreateImageSetContracts();
        _mockHttp.When("/api/imagesets")
            .Respond(HttpStatusCode.OK, "application/json", ToJson(contracts));

        var result = await _cut.ListImageSetsAsync(default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
        result.Value.Should().BeEquivalentTo(contracts);
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnFail_WhenInvalidJsonIsReturned()
    {
        var contracts = CreateImageSetContracts();
        _mockHttp.When("/api/imagesets")
            .Respond(HttpStatusCode.OK, "application/json", "invalid json");

        var result = await _cut.ListImageSetsAsync(default);

        result.IsFailed.Should().BeTrue();
    }

    private string CreateErrorJson()
    {
        var error = new ErrorResponse(500, new List<ErrorValue> { new ErrorValue("Something went wrong") });

        return JsonSerializer.Serialize(error);
    }

    private List<ImageSetContract> CreateImageSetContracts()
    {
        var contracts = Enumerable.Range(1, 10).Select(x => new ImageSetContract
        {
            Title = $"Title {x}",
            Description = $"Description {x}",
            Tags = $"Tags {x}",
            Id = Guid.NewGuid(),
            LowerYearRange = 1900,
            UpperYearRange = 2000,
            ImageCount = 10
        }).ToList();

        return contracts;
    }
    private string ToJson<T>(T data)
    {
        var json = JsonSerializer.Serialize(data, _jsonSerializerOptions);
        return json;
    }
}