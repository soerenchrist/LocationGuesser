using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Data.InMemory;
using LocationGuesser.Core.Services;
using LocationGuesser.Core.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocationGuesser.Core;

public static class DependencyInjection
{
    public static void AddCoreDependencies(this IServiceCollection services, bool useInMemory = false)
    {
        if (useInMemory)
        {
            services.AddSingleton<IImageSetRepository, InMemoryImageSetRepository>();
        }
        else
        {
            services.AddScoped<IImageSetRepository, CosmosImageSetRepository>();
        }

        services.AddSingleton<ICosmosDbContainer, CosmosDbContainer>();
        services.AddScoped<IImageRepository, CosmosImageRepository>();
        services.AddScoped<IBlobContainer, AzureBlobContainer>();
        services.AddScoped<IBlobRepository, BlobRepository>();
        services.AddScoped<IImageService, ImageService>();
    }
}