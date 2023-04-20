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
        var id = Guid.NewGuid();
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Ok(new List<Image>())));

        _cut.SetId = id;
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

        _cut.SetId = id;
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeTrue();
    }

    [Fact]
    public async Task OnInitialized_ShouldSetErrorToTrue_WhenReturnsOtherError()
    {
        var id = Guid.NewGuid();
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Fail<List<Image>>("Error")));

        _cut.SetId = id;
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeFalse();
        _cut.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task OnInitialized_ShouldSetImages_WhenReturnsImages()
    {
        var id = Guid.NewGuid();
        var images = CreateImages(id);
        _gameApiService.GetGameSetAsync(id, 5, default)
            .Returns(Task.FromResult(Result.Ok(images)));

        _cut.SetId = id;
        await _cut.OnInitializedAsync();

        _cut.NotFound.Should().BeFalse();
        _cut.IsError.Should().BeFalse();
        _cut.Images.Should().BeEquivalentTo(images);
    }

    /*
        [Fact]
        public async Task OnInitialized_ShouldSetImageUrl_WhenReturnsImages()
        {
            var id = Guid.NewGuid();
            var images = CreateImages(id);
            _gameApiService.GetGameSetAsync(id, 5, default)
                .Returns(Task.FromResult(Result.Ok(images)));
            _gameApiService.GetImageContentUrl(id, 1).ReturnsForAnyArgs("url");

            _cut.SetId = id;
            await _cut.OnInitializedAsync();
            _cut.ImageUrl.Should().BeEquivalentTo("url");
        }
        */

    [Fact]
    public void Next_ShouldSetCurrentIndexToNextNumber()
    {
        _cut.Images = CreateImages(Guid.NewGuid());
        _cut.Next();

        _cut.CurrentIndex.Should().Be(1);
    }

    [Fact]
    public void Next_ShouldDoNothing_WhenImagesIsNull()
    {
        _cut.Images = CreateImages(Guid.NewGuid());
        _cut.CurrentIndex = 4;
        _cut.Next();

        _cut.CurrentIndex.Should().Be(4);
    }

    [Fact]
    public void Next_ShouldDoNothing_WhenAlreadyOnLastIndex()
    {
        _cut.Images = CreateImages(Guid.NewGuid());
        _cut.CurrentIndex = 4;
        _cut.Next();

        _cut.CurrentIndex.Should().Be(4);
    }

    [Fact]
    public void Next_ShouldSetUrlToNextUrl()
    {
        var setId = Guid.NewGuid();
        _cut.Images = CreateImages(setId);
        _gameApiService.GetImageContentUrl(setId, 1).Returns("url1");
        _gameApiService.GetImageContentUrl(setId, 2).Returns("url2");

        _cut.Next();

        _cut.ImageUrl.Should().Be("url2");
    }

    private List<Image> CreateImages(Guid setId)
    {
        return Enumerable.Range(1, 5)
            .Select(i => new Image(setId, i, 2023, 10, 20, $"Description {i}", ""))
            .ToList();
    }
}