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

    [Fact]
    public async Task OnInitialized_ShouldNotSetNotFound_WhenSetIdIsValid()
    {
        var id = Guid.NewGuid().ToString();
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Ok(new List<Image>())));

        _cut.SetSlug = id;
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeFalse();
        _cut.SetSlug.Should().Be(id);
    }

    [Fact]
    public async Task OnInitialized_ShouldSetNotFoundToTrue_WhenApiReturnsNotFound()
    {
        var id = Guid.NewGuid().ToString();
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Fail<List<Image>>(new NotFoundError("Not found"))));

        _cut.SetSlug = id;
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeTrue();
    }

    [Fact]
    public async Task OnInitialized_ShouldSetErrorToTrue_WhenReturnsOtherError()
    {
        var id = Guid.NewGuid().ToString();
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Fail<List<Image>>("Error")));

        _cut.SetSlug = id;
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeFalse();
        _cut.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task OnInitialized_ShouldSetImages_WhenReturnsImages()
    {
        var id = Guid.NewGuid().ToString();
        var images = CreateImages(id);
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Ok(images)));

        _cut.SetSlug = id;
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeFalse();
        _cut.IsError.Should().BeFalse();
        _cut.Images.Should().BeEquivalentTo(images);
    }


    [Fact]
    public void Next_ShouldSetCurrentIndexToNextNumber()
    {
        _cut.Images = CreateImages(Guid.NewGuid().ToString());
        _cut.Next();

        _cut.CurrentIndex.Should().Be(1);
    }

    [Fact]
    public void Next_ShouldDoNothing_WhenImagesIsNull()
    {
        _cut.Images = CreateImages(Guid.NewGuid().ToString());
        _cut.CurrentIndex = 4;
        _cut.Next();

        _cut.CurrentIndex.Should().Be(4);
    }

    [Fact]
    public void Next_ShouldDoNothing_WhenAlreadyOnLastIndex()
    {
        _cut.Images = CreateImages(Guid.NewGuid().ToString());
        _cut.CurrentIndex = 4;
        _cut.Next();

        _cut.CurrentIndex.Should().Be(4);
    }

    private List<Image> CreateImages(string setSlug)
    {
        return Enumerable.Range(1, 5)
            .Select(i => new Image(setSlug, i, 2023, 10, 20, $"Description {i}", "", "Url"))
            .ToList();
    }
}