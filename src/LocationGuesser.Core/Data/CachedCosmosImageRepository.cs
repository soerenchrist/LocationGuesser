using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace LocationGuesser.Core.Data;

public class CachedCosmosImageRepository : IImageRepository
{
    private readonly CosmosImageRepository _cosmosImageRepository;
    private readonly IMemoryCache _memoryCache;
    public CachedCosmosImageRepository(CosmosImageRepository cosmosImageRepository,
        IMemoryCache memoryCache)
    {
        _cosmosImageRepository = cosmosImageRepository;
        _memoryCache = memoryCache;
    }

    public Task<Result> AddImageAsync(Image image, CancellationToken cancellationToken)
    {
        return _cosmosImageRepository.AddImageAsync(image, cancellationToken);
    }

    public Task<Result> DeleteImageAsync(Image image, CancellationToken cancellationToken)
    {
        return _cosmosImageRepository.DeleteImageAsync(image, cancellationToken);
    }

    public Task<Result<Image>> GetImageAsync(string setSlug, int number, CancellationToken cancellationToken)
    {
        string key = $"image-{setSlug}-{number}";

        return _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            return _cosmosImageRepository.GetImageAsync(setSlug, number, cancellationToken);
        })!;
    }

    public Task<Result<List<Image>>> ListImagesAsync(string setSlug, CancellationToken cancellationToken)
    {
        return _cosmosImageRepository.ListImagesAsync(setSlug, cancellationToken);
    }
}