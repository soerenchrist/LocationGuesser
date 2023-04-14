using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services;
using LocationGuesser.Tests.Fakes;
using Optional;

namespace LocationGuesser.Tests.Services;

public class ImageSetServiceTests
{
    private readonly ImageSetService _cut;
    private readonly FakeImageSetRepository _fakeImageSetRepository = new();

    public ImageSetServiceTests()
    {
        this._cut = new ImageSetService(_fakeImageSetRepository);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnNone_WhenSetWithGivenIdDoesNotExist()
    {
        var result = await this._cut.GetImageSetAsync(Guid.NewGuid(), default);
        result.Should().Be(Option.None<ImageSet>());
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnImageSet_WhenSetWithGivenIdExists()
    {
        var imageSet = CreateImageSet();
        _fakeImageSetRepository.AddImageSet(imageSet);

        var result = await this._cut.GetImageSetAsync(imageSet.Id, default);
        result.Should().Be(Option.Some(imageSet));
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnImagesFromRepository()
    {
        var imageSet1 = CreateImageSet();
        var imageSet2 = CreateImageSet();

        _fakeImageSetRepository.AddImageSet(imageSet1);
        _fakeImageSetRepository.AddImageSet(imageSet2);

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