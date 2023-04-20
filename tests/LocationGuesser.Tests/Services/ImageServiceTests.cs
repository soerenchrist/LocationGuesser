using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services;
using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Tests.Services;

public class ImageServiceTests
{
    private readonly IBlobRepository _blobRepository = Substitute.For<IBlobRepository>();
    private readonly ImageService _cut;
    private readonly IImageRepository _imageRepository = Substitute.For<IImageRepository>();
    private readonly IImageSetRepository _imageSetRepository = Substitute.For<IImageSetRepository>();
    private readonly IRandom _random = Substitute.For<IRandom>();

    public ImageServiceTests()
    {
        _random.Next(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<HashSet<int>>())
            .Returns(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        _cut = new ImageService(_blobRepository, _imageRepository, _imageSetRepository, _random);
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

    [Fact]
    public async Task GetGameSetAsync_ShouldReturnFail_WhenImageSetDoesNotExist()
    {
        var imageSetId = Guid.NewGuid();
        _imageSetRepository.GetImageSetAsync(imageSetId, default)
            .Returns(Task.FromResult(Result.Fail<ImageSet>("Not found")));

        var result = await _cut.GetGameSetAsync(imageSetId, 5, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetGameSetAsync_ShouldReturnFail_WhenImageSetDoesNotHaveEnoughImages()
    {
        var imageSet = CreateImageSet(3);
        var imageSetId = imageSet.Id;
        _imageSetRepository.GetImageSetAsync(imageSetId, default)
            .Returns(Task.FromResult(Result.Ok(imageSet)));

        var result = await _cut.GetGameSetAsync(imageSetId, 5, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetGameSetAsync_ShouldReturn5DistinctImages_WhenImageSetHasEnoughImages()
    {
        var imageSet = CreateImageSet(10);
        var imageSetId = imageSet.Id;
        _imageSetRepository.GetImageSetAsync(imageSetId, default)
            .Returns(Task.FromResult(Result.Ok(imageSet)));
        _imageRepository.GetImageAsync(imageSetId, Arg.Any<int>(), default)
            .Returns(x => Task.FromResult(Result.Ok(CreateImage(x.Arg<Guid>(), x.Arg<int>()))));

        var result = await _cut.GetGameSetAsync(imageSetId, 5, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
        result.Value.Select(i => i.Number).Distinct().Should().HaveCount(5);
    }

    [Fact]
    public async Task GetGameSetAsync_ShouldReturnFail_WhenImageFailsToFetch()
    {
        var imageSet = CreateImageSet(10);
        var imageSetId = imageSet.Id;
        _imageSetRepository.GetImageSetAsync(imageSetId, default)
            .Returns(Task.FromResult(Result.Ok(imageSet)));
        _imageRepository.GetImageAsync(imageSetId, Arg.Any<int>(), default)
            .Returns(x => Task.FromResult(Result.Fail<Image>("Failed")));

        var result = await _cut.GetGameSetAsync(imageSetId, 5, default);

        result.IsFailed.Should().BeTrue();
    }

    private ImageSet CreateImageSet(int imageCount = 10)
    {
        return new ImageSet(Guid.NewGuid(), "Title", "Description", "Tags", 1900, 2000, imageCount);
    }

    private Image CreateImage(Guid guid = default, int number = 1)
    {
        if (guid == default) guid = Guid.NewGuid();
        return new Image(guid, number, 1900, 49, 10, "Description", "Licence");
    }
}