using System.Net;
using Azure.Identity;
using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace LocationGuesser.Core.Data;

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

    public async Task<Result<ImageSet?>> GetImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        var database = _client.GetDatabase(_options.DatabaseName);
        var container = database.GetContainer(_options.ContainerName);
        try
        {
            var item = await container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey("IMAGESETS"),
                cancellationToken: cancellationToken);
            if (item.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Ok<ImageSet?>(null);
            }

            return item.Resource.ToImageSet();
        }
        catch (CosmosException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        var database = _client.GetDatabase(_options.DatabaseName);
        var container = database.GetContainer(_options.ContainerName);
        try
        {
            var query = new QueryDefinition("SELECT * FROM c where c.type = 'ImageSet'");

            using FeedIterator<ImageSet> feed = container.GetItemQueryIterator<ImageSet>(
                queryDefinition: query
            );
            var items = new List<ImageSet>();
            while (feed.HasMoreResults)
            {
                FeedResponse<ImageSet> response = await feed.ReadNextAsync();
                foreach (var item in response)
                {
                    items.Add(item);
                }
            }

            return Result.Ok(items);
        }
        catch (CosmosException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> AddImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        var database = _client.GetDatabase(_options.DatabaseName);
        var container = database.GetContainer(_options.ContainerName);
        try
        {
            var response = await container.CreateItemAsync(CosmosImageSet.FromImageSet(imageSet),
                cancellationToken: cancellationToken);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                Result.Fail($"Failed to create image set with status code {response.StatusCode}");
            }
        }
        catch (CosmosException ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}