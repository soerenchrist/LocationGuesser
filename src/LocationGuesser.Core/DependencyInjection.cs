using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LocationGuesser.Core;

public static class DependencyInjection
{
    public static void AddCoreDependencies(this IServiceCollection services)
    {
        services.AddScoped<IImageSetRepository, CosmosImageSetRepository>();
        services.AddScoped<ICosmosDbContainer, CosmosDbContainer>();
    }

}