using Azure.Identity;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Options;
using LocationGuesser.Core.Services.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace LocationGuesser.Core.Services;

public class CosmosImageSetRepository : IImageSetRepository, IDisposable
{
    private readonly CosmosClient _client;
    private readonly CosmosDbOptions _options;

    public CosmosImageSetRepository(IOptions<CosmosDbOptions> cosmosDbOptions)
    {
        _options = cosmosDbOptions.Value;
        _client = new CosmosClient(
            _options.Endpoint,
            tokenCredential: new DefaultAzureCredential());
    }

    public Task<ImageSet?> GetImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        var database = _client.GetDatabase(_options.DatabaseName);
        var container = database.GetContainer(_options.ContainerName);
        throw new NotImplementedException();
    }

    public Task<List<ImageSet>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}