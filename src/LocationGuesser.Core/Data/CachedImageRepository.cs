using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace LocationGuesser.Core.Data;

public class CachedImageRepository : IImageRepository
{
    private const int ExpirationMinutes = 1;
    private readonly IImageRepository _imageRepository;
    private readonly IMemoryCache _memoryCache;

    public CachedImageRepository(IImageRepository imageRepository,
        IMemoryCache memoryCache)
    {
        _imageRepository = imageRepository;
        _memoryCache = memoryCache;
    }

    public Task<Result> AddImageAsync(Image image, CancellationToken cancellationToken)
    {
        return _imageRepository.AddImageAsync(image, cancellationToken);
    }

    public Task<Result> DeleteImageAsync(Image image, CancellationToken cancellationToken)
    {
        return _imageRepository.DeleteImageAsync(image, cancellationToken);
    }

    public async Task<Result<Image>> GetImageAsync(string setSlug, int number, CancellationToken cancellationToken)
    {
        var key = $"image-{setSlug}-{number}";

        if (!_memoryCache.TryGetValue<Image>(key, out var image))
        {
            var result = await _imageRepository.GetImageAsync(setSlug, number, cancellationToken);
            if (result.IsFailed) return result;
            using var cacheEntry = _memoryCache.CreateEntry(key);
            cacheEntry.Value = result.Value;
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(ExpirationMinutes));
            return result;
        }

        return Result.Ok(image!);
    }

    public Task<Result<List<Image>>> ListImagesAsync(string setSlug, CancellationToken cancellationToken)
    {
        return _imageRepository.ListImagesAsync(setSlug, cancellationToken);
    }
}