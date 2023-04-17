using Microsoft.Azure.Cosmos;

namespace LocationGuesser.Core.Data.Abstractions;

public interface ICosmosDbContainer 
{
    Task<ItemResponse<T>> ReadItemAsync<T>(string id, PartitionKey partitionKey, CancellationToken cancellationToken);
    FeedIterator<T> GetItemQueryIterator<T>(QueryDefinition queryDefinition);
    Task<ItemResponse<T>> CreateItemAsync<T>(T item, CancellationToken cancellationToken);
}