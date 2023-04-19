using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services;

namespace LocationGuesser.Tests.Services;

public class ImageServiceTests
{
    private readonly IBlobRepository _blobRepository = Substitute.For<IBlobRepository>();
    private readonly ImageService _cut;
    private readonly IImageRepository _imageRepository = Substitute.For<IImageRepository>();
    private readonly IImageSetRepository _imageSetRepository = Substitute.For<IImageSetRepository>();

    public ImageServiceTests()
    {
        _cut = new ImageService(_blobRepository, _imageRepository, _imageSetRepository);
    }

    [Fact]
    public async Task AddImageToImageSetAsync_ShouldReturnFail_WhenBlobUploadFails()
    {
        var imageSet = CreateImageSet();
        var image = CreateImage();
        var filename = $"{imageSet.Id}_{image.Number}.png";
        _blobRepository.UploadImageAsync(filename, Stream.Null, default)
            .Returns(Task.FromResult(Result.Fail("Failed to upload")));

        var result = await _cut.AddImageToImageSetAsync(imageSet, image, Stream.Null, default);

        result.IsFailed.Should().BeTrue();

        await _imageRepository.DidNotReceive().AddImageAsync(image, default);
        await _imageSetRepository.DidNotReceive().UpdateImageSetAsync(imageSet, default);
    }

    [Fact]
    public async Task AddImageToImageSetAsync_ShouldReturnFail_WhenImageCreationFails()
    {
        var imageSet = CreateImageSet();
        var image = CreateImage();
        var filename = $"{imageSet.Id}_{image.Number}.png";
        _blobRepository.UploadImageAsync(filename, Stream.Null, default)
            .Returns(Task.FromResult(Result.Ok()));
        _imageRepository.AddImageAsync(image, default)
            .Returns(Task.FromResult(Result.Fail("Something went wrong")));

        var result = await _cut.AddImageToImageSetAsync(imageSet, image, Stream.Null, default);

        result.IsFailed.Should().BeTrue();

        await _imageSetRepository.DidNotReceive().UpdateImageSetAsync(imageSet, default);
    }

    [Fact]
    public async Task AddImageToImageSetAsync_ShouldTryToDeleteBlob_WhenImageCreationFails()
    {
        var imageSet = CreateImageSet();
        var image = CreateImage();
        var filename = $"{imageSet.Id}_{image.Number}.png";
        _blobRepository.UploadImageAsync(filename, Stream.Null, default)
            .Returns(Task.FromResult(Result.Ok()));
        _imageRepository.AddImageAsync(image, default)
            .Returns(Task.FromResult(Result.Fail("Something went wrong")));

        var result = await _cut.AddImageToImageSetAsync(imageSet, image, Stream.Null, default);

        await _blobRepository.Received(1).DeleteImageAsync(filename, default);
    }

    [Fact]
    public async Task AddImageToImageSetAsync_ShouldReturnFail_WhenImageSetUpdateFails()
    {
        var imageSet = CreateImageSet();
        var image = CreateImage();
        var filename = $"{imageSet.Id}_{image.Number}.png";
        _blobRepository.UploadImageAsync(filename, Stream.Null, default)
            .Returns(Task.FromResult(Result.Ok()));
        _imageRepository.AddImageAsync(image, default)
            .Returns(Task.FromResult(Result.Ok()));
        _imageSetRepository.UpdateImageSetAsync(imageSet with { ImageCount = imageSet.ImageCount + 1 }, default)
            .Returns(Task.FromResult(Result.Fail("Failed")));

        var result = await _cut.AddImageToImageSetAsync(imageSet, image, Stream.Null, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageToImageSetAsync_ShouldTryToDeleteBlobAndImage_WhenImageSetUpdateFails()
    {
        var imageSet = CreateImageSet();
        var image = CreateImage();
        var filename = $"{imageSet.Id}_{image.Number}.png";
        _blobRepository.UploadImageAsync(filename, Stream.Null, default)
            .Returns(Task.FromResult(Result.Ok()));
        _imageRepository.AddImageAsync(image, default)
            .Returns(Task.FromResult(Result.Ok()));
        _imageSetRepository.UpdateImageSetAsync(imageSet with { ImageCount = imageSet.ImageCount + 1 }, default)
            .Returns(Task.FromResult(Result.Fail("Failed")));

        var result = await _cut.AddImageToImageSetAsync(imageSet, image, Stream.Null, default);

        await _blobRepository.Received(1).DeleteImageAsync(filename, default);
        await _imageRepository.Received(1).DeleteImageAsync(image, default);
    }

    private ImageSet CreateImageSet()
    {
        return new ImageSet(Guid.NewGuid(), "Title", "Description", "Tags", 1900, 2000, 10);
    }

    private Image CreateImage()
    {
        return new Image(Guid.NewGuid(), 1, 1900, 49, 10, "Description", "Licence");
    }
}