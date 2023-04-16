using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services;

namespace LocationGuesser.Tests.Services;

public class ImageSetServiceTests
{
    private readonly ImageSetService _cut;
    private readonly IImageSetRepository _imageSetRepo = Substitute.For<IImageSetRepository>();

    public ImageSetServiceTests()
    {
        this._cut = new ImageSetService(_imageSetRepo);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnNone_WhenSetWithGivenIdDoesNotExist()
    {
        _imageSetRepo.GetImageSetAsync(Arg.Any<Guid>(), default).Returns(Result.Ok<ImageSet?>(null));
        var result = await this._cut.GetImageSetAsync(Guid.NewGuid(), default);
        result.Should().BeEquivalentTo(Result.Ok<ImageSet?>(null));
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnFail_WhenCallToRepositoryFails()
    {
        _imageSetRepo.GetImageSetAsync(Arg.Any<Guid>(), default).Returns(Result.Fail<ImageSet?>("Some error"));
        var result = await this._cut.GetImageSetAsync(Guid.NewGuid(), default);
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeEquivalentTo(new Error("Some error"));
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnImageSet_WhenSetWithGivenIdExists()
    {
        var imageSet = CreateImageSet();
        _imageSetRepo.GetImageSetAsync(Arg.Any<Guid>(), default).Returns(Result.Ok<ImageSet?>(imageSet));

        var result = await this._cut.GetImageSetAsync(imageSet.Id, default);
        result.Should().BeEquivalentTo(Result.Ok<ImageSet?>(imageSet));
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnImagesFromRepository()
    {
        var imageSet1 = CreateImageSet();
        var imageSet2 = CreateImageSet();
        _imageSetRepo.ListImageSetsAsync(default).Returns(new List<ImageSet> { imageSet1, imageSet2 });

        var result = await this._cut.ListImageSetsAsync(default);
        result.Should().BeEquivalentTo(new List<ImageSet> { imageSet1, imageSet2 });
    }

    private ImageSet CreateImageSet()
    {
        var commerce = new Bogus.DataSets.Commerce();
        return new ImageSet(Guid.NewGuid(), commerce.Product(), commerce.ProductDescription(),
            commerce.ProductAdjective());
    }
}