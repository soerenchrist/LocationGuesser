using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Data.InMemory;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Tests.Data.InMemory;

public class InMemoryImageRepositoryTests
{
    private readonly InMemoryImageRepository _cut;

    public InMemoryImageRepositoryTests()
    {
        _cut = new InMemoryImageRepository();
    }

    [Fact]
    public async Task ListImagesAsync_ReturnsEmptyList_WhenNoImagesExist()
    {
        var result = await _cut.ListImagesAsync(Guid.NewGuid().ToString(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task AddImageAsync_ShouldAddImageToRepo()
    {
        var image = CreateImage();
        var addResult = await _cut.AddImageAsync(image, default);
        addResult.IsSuccess.Should().BeTrue();

        var result = await _cut.ListImagesAsync(Guid.NewGuid().ToString(), default);

        result.Value.Should().Contain(image);
    }

    [Fact]
    public async Task AddImageAsync_ShouldReturnFail_WhenSetIdAndNumberExist()
    {
        var image = CreateImage();
        var addResult = await _cut.AddImageAsync(image, default);
        addResult.IsSuccess.Should().BeTrue();

        var secondImage = new Image(image.SetSlug, image.Number, 2000, 50, 1, "", "", "");

        var result = await _cut.AddImageAsync(secondImage, default);
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnNotFound_WhenImageDoesNotExist()
    {
        var result = await _cut.GetImageAsync(Guid.NewGuid().ToString(), 1, default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnImage_WhenItExists()
    {
        var image = CreateImage();
        await _cut.AddImageAsync(image, default);
        var result = await _cut.GetImageAsync(image.SetSlug, image.Number, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(image);
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnFail_WhenImageDoesNotExist()
    {
        var result = await _cut.DeleteImageAsync(CreateImage(), default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldDeleteImage_WhenImageDoesExist()
    {
        var image = CreateImage();
        await _cut.AddImageAsync(image, default);

        var result = await _cut.DeleteImageAsync(image, default);

        result.IsSuccess.Should().BeTrue();
    }

    private Image CreateImage()
    {
        return new Image("slug", 1, 1954, 49, 10, "Description", "License", "Url");
    }
}