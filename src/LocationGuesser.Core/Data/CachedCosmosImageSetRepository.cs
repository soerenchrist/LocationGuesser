using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace LocationGuesser.Core.Data;

public class CachedCosmosImageSetRepository : IImageSetRepository
{
    private readonly CosmosImageSetRepository _cosmosImageSetRepository;
    private readonly IMemoryCache _memoryCache;

    public CachedCosmosImageSetRepository(CosmosImageSetRepository cosmosImageSetRepository,
        IMemoryCache memoryCache)
    {
        _cosmosImageSetRepository = cosmosImageSetRepository;
        _memoryCache = memoryCache;
    }

    public Task<Result<ImageSet>> AddImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        return _cosmosImageSetRepository.AddImageSetAsync(imageSet, cancellationToken);
    }

    public Task<Result> DeleteImageSetAsync(string slug, CancellationToken cancellationToken)
    {
        return _cosmosImageSetRepository.DeleteImageSetAsync(slug, cancellationToken);
    }

    public Task<Result<ImageSet>> GetImageSetAsync(string slug, CancellationToken cancellationToken)
    {
        var key = $"imageset-{slug}";

        return _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            return _cosmosImageSetRepository.GetImageSetAsync(slug, cancellationToken);
        })!;
    }

    public Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        var key = "imagesets";

        return _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            return _cosmosImageSetRepository.ListImageSetsAsync(cancellationToken);
        })!;
    }

    public Task<Result> UpdateImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        return _cosmosImageSetRepository.UpdateImageSetAsync(imageSet, cancellationToken);
    }
}