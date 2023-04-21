using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Data.InMemory;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Tests.Data.InMemory;

public class InMemoryImageSetRepositoryTests
{
    private readonly InMemoryImageSetRepository _cut;

    public InMemoryImageSetRepositoryTests()
    {
        _cut = new InMemoryImageSetRepository();
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnImageSet_WhenItExists()
    {
        var imageSet = new ImageSet(Guid.NewGuid().ToString(), "Title", "Description", "Tags", 1900, 2000, 0);
        await _cut.AddImageSetAsync(imageSet, CancellationToken.None);

        var result = await _cut.GetImageSetAsync(imageSet.Slug, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(imageSet);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnNotFound_WhenImageSetDoesNotExist()
    {
        var result = await _cut.GetImageSetAsync(Guid.NewGuid().ToString(), default);
        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnEmptyList_WhenNoImageSetsExist()
    {
        var result = await _cut.ListImageSetsAsync(default);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnItems_WhenImageSetsExist()
    {
        var imagesSets = CreateImageSets();
        foreach (var imageSet in imagesSets) await _cut.AddImageSetAsync(imageSet, default);

        var result = await _cut.ListImageSetsAsync(default);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(imagesSets);
    }

    [Fact]
    public async Task AddImageSetAsync_ShouldAddImageSet()
    {
        var imageSet = new ImageSet(Guid.NewGuid().ToString(), "Title", "Description", "Tags", 1900, 2000, 0);
        var result = await _cut.AddImageSetAsync(imageSet, default);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(imageSet);

        var listResult = await _cut.ListImageSetsAsync(default);
        listResult.Value.Should().Contain(imageSet);
    }

    [Fact]
    public async Task AddImageSetAsync_ShouldReturnFail_WhenIdAlreadyExists()
    {
        var imageSet = CreateImageSets(1).First();
        await _cut.AddImageSetAsync(imageSet, default);

        var result = await _cut.AddImageSetAsync(imageSet, default);
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateImageSetAsync_ShouldUpdateImageSet()
    {
        var imageSet = CreateImageSets(1).First();
        await _cut.AddImageSetAsync(imageSet, default);

        var updatedImageSet = imageSet with { Title = "Title 2" };
        var result = await _cut.UpdateImageSetAsync(updatedImageSet, default);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _cut.GetImageSetAsync(imageSet.Slug, default);
        getResult.Value.Should().Be(updatedImageSet);
    }

    [Fact]
    public async Task UpdateImageSetAsync_ShouldReturnFail_WhenIdDoesNotExist()
    {
        var imageSet = CreateImageSets(1).First();
        await _cut.AddImageSetAsync(imageSet, default);

        var updatedImageSet = CreateImageSets(1).First();
        var result = await _cut.UpdateImageSetAsync(updatedImageSet, default);
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageSetAsync_ShouldDeleteImage()
    {
        var imageSet = CreateImageSets(1).First();
        await _cut.AddImageSetAsync(imageSet, default);

        var result = await _cut.DeleteImageSetAsync(imageSet.Slug, default);
        result.IsSuccess.Should().BeTrue();

        var listResult = await _cut.ListImageSetsAsync(default);
        listResult.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteImageSetAsync_ShouldReturnFail_WhenImageSetDoesNotExist()
    {
        var imageSet = CreateImageSets(1).First();
        await _cut.AddImageSetAsync(imageSet, default);

        var result = await _cut.DeleteImageSetAsync(Guid.NewGuid().ToString(), default);
        result.IsFailed.Should().BeTrue();
    }

    private List<ImageSet> CreateImageSets(int count = 10)
    {
        return Enumerable.Range(1, count)
            .Select(x => new ImageSet(Guid.NewGuid().ToString(), $"Title{x}", $"Description{x}", $"Tags{x}", 1900, 2000, 0))
            .ToList();
    }
}