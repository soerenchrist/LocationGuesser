using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace LocationGuesser.Core.Data;

public class CachedImageSetRepository : IImageSetRepository
{
    private readonly IImageSetRepository _imageSetRepository;
    private readonly IMemoryCache _memoryCache;
    private const int ExpirationMinutes = 5;

    public CachedImageSetRepository(IImageSetRepository imageSetRepository,
        IMemoryCache memoryCache)
    {
        _imageSetRepository = imageSetRepository;
        _memoryCache = memoryCache;
    }

    public Task<Result<ImageSet>> AddImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        return _imageSetRepository.AddImageSetAsync(imageSet, cancellationToken);
    }

    public Task<Result> DeleteImageSetAsync(string slug, CancellationToken cancellationToken)
    {
        return _imageSetRepository.DeleteImageSetAsync(slug, cancellationToken);
    }

    public async Task<Result<ImageSet>> GetImageSetAsync(string slug, CancellationToken cancellationToken)
    {
        var key = $"imageset-{slug}";

        if (!_memoryCache.TryGetValue<ImageSet>(key, out var imageSet))
        {
            var result = await _imageSetRepository.GetImageSetAsync(slug, cancellationToken);
            if (result.IsFailed) return result;
            using var cacheEntry = _memoryCache.CreateEntry(key);
            cacheEntry.Value = result.Value;
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(ExpirationMinutes));
            return result;
        }

        return Result.Ok(imageSet!);
    }

    public async Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        var key = "imagesets";

        if (!_memoryCache.TryGetValue<List<ImageSet>>(key, out var imageSets))
        {
            var result = await _imageSetRepository.ListImageSetsAsync(cancellationToken);
            if (result.IsFailed) return result;
            using var cacheEntry = _memoryCache.CreateEntry(key);
            cacheEntry.Value = result.Value;
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(ExpirationMinutes));
            return result;
        }

        return Result.Ok(imageSets!);
    }

    public Task<Result> UpdateImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        return _imageSetRepository.UpdateImageSetAsync(imageSet, cancellationToken);
    }
}