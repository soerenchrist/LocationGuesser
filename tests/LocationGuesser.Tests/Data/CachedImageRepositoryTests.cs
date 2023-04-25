using FluentResults;
using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace LocationGuesser.Tests.Data;

public class CachedImageRepositoryTests
{
    private readonly CachedImageRepository _cut;
    private readonly IImageRepository _imageRepository = Substitute.For<IImageRepository>();
    private readonly IMemoryCache _memoryCache = Substitute.For<IMemoryCache>();
    private readonly ICacheEntry _cacheEntry = Substitute.For<ICacheEntry>();

    private const string Slug = "test_slug";
    private const int ImageNumber = 3;
    private readonly string _key = $"image-{Slug}-{ImageNumber}";

    public CachedImageRepositoryTests()
    {
        _memoryCache.CreateEntry(_key).Returns(_cacheEntry);
        _cut = new CachedImageRepository(_imageRepository, _memoryCache);
    }

    [Fact]
    public async Task GetImageAsync_ShouldFetchFromRepository_WhenCacheIsEmpty()
    {
        var image = CreateImage();
        _memoryCache.TryGetValue(_key, out Arg.Any<Image>()!)
            .Returns(false);
        _imageRepository.GetImageAsync(Slug, ImageNumber, default)
            .Returns(Task.FromResult(Result.Ok(image)));
        await _cut.GetImageAsync(Slug, ImageNumber, default);

        await _imageRepository.Received(1).GetImageAsync(Slug, ImageNumber, default);
    }

    [Fact]
    public async Task GetImageAsync_ShouldAddValueToCache_WhenCacheIsEmptyAndRepositoryReturnsSuccess()
    {
        var image = CreateImage();
        _memoryCache.TryGetValue(_key, out Arg.Any<Image>()!)
            .Returns(false);
        _imageRepository.GetImageAsync(Slug, ImageNumber, default)
            .Returns(Task.FromResult(Result.Ok(image)));

        await _cut.GetImageAsync(Slug, ImageNumber, default);

        _memoryCache.Received(1).CreateEntry(_key);
        _cacheEntry.Value.Should().Be(image);
        _cacheEntry.Received(1).SetAbsoluteExpiration(Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetImageAsync_ShouldNotAddValueToCache_WhenRepositoryCallFails()
    {
        _memoryCache.TryGetValue(_key, out Arg.Any<Image>()!)
            .Returns(false);

        _imageRepository.GetImageAsync(Slug, ImageNumber, default)
            .Returns(Task.FromResult(Result.Fail<Image>("Something failed")));

        await _cut.GetImageAsync(Slug, ImageNumber, default);

        _memoryCache.DidNotReceive().CreateEntry(_key);
    }

    [Fact]
    public async Task GetImageAsync_ShouldNotCallRepository_WhenCacheContainsItem()
    {
        var image = CreateImage();
        _memoryCache.TryGetValue(_key, out Arg.Any<Image>()!)
            .Returns(x =>
            {
                x[1] = image;
                return true;
            });

        await _cut.GetImageAsync(Slug, ImageNumber, default);

        await _imageRepository.DidNotReceiveWithAnyArgs().GetImageAsync(Arg.Any<string>(), Arg.Any<int>(), default);
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnItemFromCache_WhenItExists()
    {
        var image = CreateImage();
        _memoryCache.TryGetValue(_key, out Arg.Any<Image>()!)
            .Returns(x =>
            {
                x[1] = image;
                return true;
            });
        var result = await _cut.GetImageAsync(Slug, ImageNumber, default);
        result.Value.Should().Be(image);
        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnItemFromRepository_WhenCacheIsEmpty()
    {
        var image = CreateImage();
        _memoryCache.TryGetValue(_key, out Arg.Any<Image>()!)
            .Returns(false);
        _imageRepository.GetImageAsync(Slug, ImageNumber, default)
            .Returns(Task.FromResult(Result.Ok(image)));
        var result = await _cut.GetImageAsync(Slug, ImageNumber, default);
        result.Value.Should().Be(image);
        result.IsSuccess.Should().Be(true);
    }

    private Image CreateImage()
    {
        return new Image(Slug, ImageNumber, 2022, 49, 20, "", "", "");
    }
}