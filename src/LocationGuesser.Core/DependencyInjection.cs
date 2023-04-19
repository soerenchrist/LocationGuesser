using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Services;
using LocationGuesser.Core.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LocationGuesser.Core;

public static class DependencyInjection
{
    public static void AddCoreDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ICosmosDbContainer, CosmosDbContainer>();
        services.AddScoped<IImageSetRepository, CosmosImageSetRepository>();
        services.AddScoped<IImageRepository, CosmosImageRepository>();
        services.AddScoped<IBlobContainer, AzureBlobContainer>();
        services.AddScoped<IBlobRepository, BlobRepository>();
        services.AddScoped<IImageService, ImageService>();
    }
}