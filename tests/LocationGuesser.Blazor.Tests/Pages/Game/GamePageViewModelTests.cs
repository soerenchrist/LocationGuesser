using FluentResults;
using LocationGuesser.Blazor.Pages.Game;
using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Blazor.Tests.Pages.Game;

public class GamePageViewModelTests
{
    private readonly GamePageViewModel _cut;
    private readonly IGameApiService _gameApiService = Substitute.For<IGameApiService>();

    public GamePageViewModelTests()
    {
        _cut = new GamePageViewModel(_gameApiService);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid guid")]
    public async Task OnInitialized_ShouldSetNotFoundToTrue_WhenSetIdIsInvalid(string? setId)
    {
        _cut.SetIdString = setId;
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeTrue();
    }

    [Fact]
    public async Task OnInitialized_ShouldNotSetNotFound_WhenSetIdIsValid()
    {
        var id = Guid.NewGuid();
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Ok(new List<Image>())));

        _cut.SetIdString = id.ToString();
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeFalse();
        _cut.SetId.Should().Be(id);
    }

    [Fact]
    public async Task OnInitialized_ShouldSetNotFoundToTrue_WhenApiReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Fail<List<Image>>(new NotFoundError("Not found"))));

        _cut.SetIdString = id.ToString();
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeTrue();
    }

    [Fact]
    public async Task OnInitialized_ShouldSetErrorToTrue_WhenReturnsOtherError()
    {
        var id = Guid.NewGuid();
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Fail<List<Image>>("Error")));

        _cut.SetIdString = id.ToString();
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeFalse();
        _cut.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task OnInitialized_ShouldSetImages_WhenReturnsImages()
    {
        var id = Guid.NewGuid();
        var images = CreateImages();
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Ok(images)));

        _cut.SetIdString = id.ToString();
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeFalse();
        _cut.IsError.Should().BeFalse();
        _cut.Images.Should().BeEquivalentTo(images);
    }

    private List<Image> CreateImages()
    {
        return Enumerable.Range(1, 5)
            .Select(i => new Image(Guid.NewGuid(), i, 2023, 10, 20, $"Description {i}", ""))
            .ToList();
    }
}