using FluentResults;
using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace LocationGuesser.Tests.Data;

public class CachedImageSetRepositoryTests
{
    private readonly CachedImageSetRepository _cut;
    private readonly IImageSetRepository _imageSetRepository = Substitute.For<IImageSetRepository>();
    private readonly IMemoryCache _memoryCache = Substitute.For<IMemoryCache>();
    private readonly ICacheEntry _cacheEntry = Substitute.For<ICacheEntry>();

    private const string Slug = "test_slug";
    private readonly string _key = $"imageset-{Slug}";

    public CachedImageSetRepositoryTests()
    {
        _memoryCache.CreateEntry(_key).ReturnsForAnyArgs(_cacheEntry);
        _cut = new CachedImageSetRepository(_imageSetRepository, _memoryCache);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldFetchFromRepository_WhenCacheIsEmpty()
    {
        var imageSet = CreateImageSet();
        _memoryCache.TryGetValue(_key, out Arg.Any<ImageSet>()!)
            .Returns(false);
        _imageSetRepository.GetImageSetAsync(Slug, default)
            .Returns(Task.FromResult(Result.Ok(imageSet)));
        await _cut.GetImageSetAsync(Slug, default);

        await _imageSetRepository.Received(1).GetImageSetAsync(Slug, default);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldAddValueToCache_WhenCacheIsEmptyAndRepositoryReturnsSuccess()
    {
        var imageSet = CreateImageSet();
        _memoryCache.TryGetValue(_key, out Arg.Any<ImageSet>()!)
            .Returns(false);
        _imageSetRepository.GetImageSetAsync(Slug, default)
            .Returns(Task.FromResult(Result.Ok(imageSet)));

        await _cut.GetImageSetAsync(Slug, default);

        _memoryCache.Received(1).CreateEntry(_key);
        _cacheEntry.Value.Should().Be(imageSet);
        _cacheEntry.Received(1).SetAbsoluteExpiration(Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldNotAddValueToCache_WhenRepositoryCallFails()
    {
        _memoryCache.TryGetValue(_key, out Arg.Any<ImageSet>()!)
            .Returns(false);

        _imageSetRepository.GetImageSetAsync(Slug, default)
            .Returns(Task.FromResult(Result.Fail<ImageSet>("Something failed")));

        await _cut.GetImageSetAsync(Slug, default);

        _memoryCache.DidNotReceive().CreateEntry(_key);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldNotCallRepository_WhenCacheContainsItem()
    {
        var imageSet = CreateImageSet();
        _memoryCache.TryGetValue(_key, out Arg.Any<ImageSet>()!)
            .Returns(x =>
            {
                x[1] = imageSet;
                return true;
            });

        await _cut.GetImageSetAsync(Slug, default);

        await _imageSetRepository.DidNotReceiveWithAnyArgs().GetImageSetAsync(Arg.Any<string>(), default);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnItemFromCache_WhenItExists()
    {
        var imageSet = CreateImageSet();
        _memoryCache.TryGetValue(_key, out Arg.Any<ImageSet>()!)
            .Returns(x =>
            {
                x[1] = imageSet;
                return true;
            });
        var result = await _cut.GetImageSetAsync(Slug, default);
        result.Value.Should().Be(imageSet);
        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnItemFromRepository_WhenCacheIsEmpty()
    {
        var imageSet = CreateImageSet();
        _memoryCache.TryGetValue(_key, out Arg.Any<ImageSet>()!)
            .Returns(false);
        _imageSetRepository.GetImageSetAsync(Slug, default)
            .Returns(Task.FromResult(Result.Ok(imageSet)));
        var result = await _cut.GetImageSetAsync(Slug, default);
        result.Value.Should().Be(imageSet);
        result.IsSuccess.Should().Be(true);
    }
    
    [Fact]
    public async Task ListImageSetsAsync_ShouldFetchFromRepository_WhenCacheIsEmpty()
    {
        var imageSets = new List<ImageSet> { CreateImageSet() };
        _memoryCache.TryGetValue("imagesets", out Arg.Any<List<ImageSet>>()!)
            .Returns(false);
        _imageSetRepository.ListImageSetsAsync(default)
            .Returns(Task.FromResult(Result.Ok(imageSets)));
        await _cut.ListImageSetsAsync(default);

        await _imageSetRepository.Received(1).ListImageSetsAsync(default);
    }
    
    [Fact]
    public async Task ListImageSetsAsync_ShouldAddValueToCache_WhenCacheIsEmptyAndRepositoryReturnsSuccess()
    {
        var imageSets = new List<ImageSet> { CreateImageSet() };
        _memoryCache.TryGetValue("imagesets", out Arg.Any<List<ImageSet>>()!)
            .Returns(false);
        _imageSetRepository.ListImageSetsAsync(default)
            .Returns(Task.FromResult(Result.Ok(imageSets)));

        await _cut.ListImageSetsAsync(default);

        _memoryCache.Received(1).CreateEntry("imagesets");
        _cacheEntry.Value.Should().Be(imageSets);
        _cacheEntry.Received(1).SetAbsoluteExpiration(Arg.Any<TimeSpan>());
    }
    
    [Fact]
    public async Task ListImageSetsAsync_ShouldNotAddValueToCache_WhenRepositoryCallFails()
    {
        _memoryCache.TryGetValue("imagesets", out Arg.Any<List<ImageSet>>()!)
            .Returns(false);

        _imageSetRepository.ListImageSetsAsync(default)
            .Returns(Task.FromResult(Result.Fail<List<ImageSet>>("Something failed")));

        await _cut.ListImageSetsAsync(default);

        _memoryCache.DidNotReceive().CreateEntry("imagesets");
    }
    
    [Fact]
    public async Task ListImageSetsAsync_ShouldNotCallRepository_WhenCacheContainsItem()
    {
        var imageSets = new List<ImageSet> { CreateImageSet() };
        _memoryCache.TryGetValue("imagesets", out Arg.Any<List<ImageSet>>()!)
            .Returns(x =>
            {
                x[1] = imageSets;
                return true;
            });

        await _cut.ListImageSetsAsync(default);

        await _imageSetRepository.DidNotReceiveWithAnyArgs().ListImageSetsAsync(default);
    }
    
    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnItemFromCache_WhenItExists()
    {
        var imageSets = new List<ImageSet> { CreateImageSet() };
        _memoryCache.TryGetValue("imagesets", out Arg.Any<List<ImageSet>>()!)
            .Returns(x =>
            {
                x[1] = imageSets;
                return true;
            });
        var result = await _cut.ListImageSetsAsync(default);
        result.Value.Should().BeEquivalentTo(imageSets);
        result.IsSuccess.Should().Be(true);
    }
    
    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnItemFromRepository_WhenCacheIsEmpty()
    {
        var imageSets = new List<ImageSet> { CreateImageSet() };
        _memoryCache.TryGetValue("imagesets", out Arg.Any<List<ImageSet>>()!)
            .Returns(false);
        _imageSetRepository.ListImageSetsAsync(default)
            .Returns(Task.FromResult(Result.Ok(imageSets)));
        var result = await _cut.ListImageSetsAsync(default);
        result.Value.Should().BeEquivalentTo(imageSets);
        result.IsSuccess.Should().Be(true);
    }

    private static ImageSet CreateImageSet()
    {
        return new ImageSet(Slug, "Title", "Description", "Tags", 1900, 2000, 10);
    }
}