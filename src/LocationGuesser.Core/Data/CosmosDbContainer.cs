using Azure.Identity;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace LocationGuesser.Core.Data;

public class CosmosDbContainer : ICosmosDbContainer, IDisposable
{
    private readonly CosmosClient _client;
    private readonly Container _container;

    public CosmosDbContainer(IOptions<CosmosDbOptions> cosmosDbOptions)
    {
        var options = cosmosDbOptions.Value;
        _client = new CosmosClient(options.Endpoint, new DefaultAzureCredential());
        var database = _client.GetDatabase(options.DatabaseName);
        _container = database.GetContainer(options.ContainerName);
    }

    public Task<ItemResponse<T>> CreateItemAsync<T>(T item, CancellationToken cancellationToken)
    {
        return _container.CreateItemAsync(item, cancellationToken: cancellationToken);
    }


    public FeedIterator<T> GetItemQueryIterator<T>(QueryDefinition queryDefinition)
    {
        return _container.GetItemQueryIterator<T>(queryDefinition);
    }

    public Task<ItemResponse<T>> ReadItemAsync<T>(string id, PartitionKey partitionKey,
        CancellationToken cancellationToken)
    {
        return _container.ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken);
    }

    public Task<ItemResponse<T>> UpsertItemAsync<T>(T item, CancellationToken cancellationToken)
    {
        return _container.UpsertItemAsync(item, cancellationToken: cancellationToken);
    }

    public Task<ItemResponse<T>> DeleteItemAsync<T>(string id, PartitionKey partitionKey,
        CancellationToken cancellationToken)
    {
        return _container.DeleteItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}