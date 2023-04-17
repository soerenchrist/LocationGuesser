using System.Net;
using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LocationGuesser.Core.Data;

public class CosmosImageSetRepository : IImageSetRepository
{
    private readonly ICosmosDbContainer _container;
    private readonly ILogger<CosmosImageSetRepository> _logger;
    public CosmosImageSetRepository(ICosmosDbContainer container,
    ILogger<CosmosImageSetRepository> logger)
    {
        _container = container;
        _logger = logger;
    }

    public async Task<Result<ImageSet?>> GetImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey("IMAGESETS"),
                cancellationToken: cancellationToken);

            return (item.StatusCode, item.Resource) switch
            {
                (HttpStatusCode.NotFound, _) => Result.Ok<ImageSet?>(null),
                (HttpStatusCode.OK, null) => Result.Ok<ImageSet?>(null),
                (HttpStatusCode.OK, CosmosImageSet resource) => resource.ToImageSet(),
                _ => Result.Fail($"Unknown error occured with status code {item.StatusCode}")
            };
        }
        catch (CosmosException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c where c.type = 'ImageSet'");

            using FeedIterator<CosmosImageSet> feed = _container.GetItemQueryIterator<CosmosImageSet>(
                queryDefinition: query
            );
            var items = new List<ImageSet>();
            while (feed.HasMoreResults)
            {
                FeedResponse<CosmosImageSet> response = await feed.ReadNextAsync();
                foreach (var item in response)
                {
                    items.Add(item.ToImageSet());
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
        try
        {
            var response = await _container.CreateItemAsync(CosmosImageSet.FromImageSet(imageSet),
                cancellationToken: cancellationToken);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                return Result.Fail($"Failed to create image set with status code {response.StatusCode}");
            }
        }
        catch (CosmosException ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }
}