using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Data.Blob;
using LocationGuesser.Core.Data.Cosmos;
using LocationGuesser.Core.Services;
using LocationGuesser.Core.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace LocationGuesser.Core;

public static class DependencyInjection
{
    public static void AddCoreDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ICosmosDbContainer, CosmosDbContainer>();
        services.AddScoped<CosmosImageSetRepository>();
        services.AddScoped<IImageSetRepository>(provider => new CachedImageSetRepository(
            provider.GetRequiredService<CosmosImageSetRepository>(),
            provider.GetRequiredService<IMemoryCache>()));
        services.AddScoped<CosmosImageRepository>();
        services.AddScoped<IImageRepository>(provider => new CachedImageRepository(
            provider.GetRequiredService<CosmosImageRepository>(),
            provider.GetRequiredService<IMemoryCache>()));

        services.AddScoped<IDailyChallengeRepository, CosmosDailyChallengeRepository>();
        services.AddScoped<IDailyChallengeService, DailyChallengeService>();

        services.AddMemoryCache();

        services.AddScoped<IBlobContainer, AzureBlobContainer>();
        services.AddScoped<IBlobRepository, BlobRepository>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IRandom, RandomGenerator>();
        services.AddTransient<IImageUrlService, ImageUrlService>();
    }
}