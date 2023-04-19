using FluentResults;
using LocationGuesser.Blazor.Pages.Index;
using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Blazor.Tests.Pages.Index;

public class IndexPageViewModelTests
{
    private readonly IndexPageViewModel _cut;
    private readonly IImageSetApiService _imageSetApiService = Substitute.For<IImageSetApiService>();

    public IndexPageViewModelTests()
    {
        _cut = new IndexPageViewModel(_imageSetApiService);
    }

    [Fact]
    public async Task OnInitializedAsync_ShouldCallApiForListOfImageSets()
    {
        _imageSetApiService.ListImageSetsAsync()
            .ReturnsForAnyArgs(Task.FromResult(Result.Ok(new List<ImageSet>())));
        await _cut.OnInitializedAsync();

        await _imageSetApiService.Received(1).ListImageSetsAsync();
    }

    [Fact]
    public async Task OnInitializedAsync_ShouldSetErrorToTrue_WhenApiCallFails()
    {
        _imageSetApiService.ListImageSetsAsync()
            .ReturnsForAnyArgs(Task.FromResult(Result.Fail<List<ImageSet>>("Something failed")));

        await _cut.OnInitializedAsync();

        _cut.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task OnInitializedAsync_ShouldSetLoadingToFalse_WhenApiCallFails()
    {
        _imageSetApiService.ListImageSetsAsync()
            .ReturnsForAnyArgs(Task.FromResult(Result.Fail<List<ImageSet>>("Something failed")));

        await _cut.OnInitializedAsync();

        _cut.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task OnInitializedAsync_ShouldSetLoadingToFalse_WhenApiCallSucceeds()
    {
        _imageSetApiService.ListImageSetsAsync()
            .ReturnsForAnyArgs(Task.FromResult(Result.Ok(new List<ImageSet>())));

        await _cut.OnInitializedAsync();

        _cut.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task OnInitializedAsync_ShouldSetImageSets_WhenApiCallSucceeds()
    {
        var imageSets = CreateImageSets();
        _imageSetApiService.ListImageSetsAsync()
            .ReturnsForAnyArgs(Task.FromResult(Result.Ok(imageSets)));

        await _cut.OnInitializedAsync();

        _cut.ImageSets.Should().BeEquivalentTo(imageSets);
    }

    private List<ImageSet> CreateImageSets(int number = 10)
    {
        return Enumerable.Range(1, number)
            .Select(i => new ImageSet(Guid.NewGuid(), $"ImageSet {i}", $"Description {i}", $"Tags {i}", 1900, 2000, 1))
            .ToList();
    }
}