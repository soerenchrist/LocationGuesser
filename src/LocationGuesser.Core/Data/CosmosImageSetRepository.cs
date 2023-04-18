using System.Net;
using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Data.Dtos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Core.Data;

public class CosmosImageSetRepository : IImageSetRepository
{
    private readonly ICosmosDbContainer _container;
    private readonly ILogger<CosmosImageSetRepository> _logger;
    private const string PartitionKey = "IMAGESETS";

    public CosmosImageSetRepository(ICosmosDbContainer container,
        ILogger<CosmosImageSetRepository> logger)
    {
        _container = container;
        _logger = logger;
    }

    public async Task<Result<ImageSet>> GetImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey),
                cancellationToken: cancellationToken);

            return (item.StatusCode, item.Resource) switch
            {
                (HttpStatusCode.NotFound, _) => Result.Fail<ImageSet>(CreateNotFoundError(id)),
                (HttpStatusCode.OK, null) => Result.Fail<ImageSet>(CreateNotFoundError(id)),
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

    public async Task<Result> UpdateImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.UpsertItemAsync(CosmosImageSet.FromImageSet(imageSet), cancellationToken);
            return response.StatusCode switch
            {
                HttpStatusCode.OK or HttpStatusCode.NoContent => Result.Ok(),
                HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(imageSet.Id)),
                _ => Result.Fail($"Unknown error occured with status code {response.StatusCode}")
            };
        }
        catch (CosmosException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> DeleteImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.DeleteItemAsync<CosmosImageSet>(id.ToString(),
                new PartitionKey(PartitionKey),
                cancellationToken: cancellationToken);
            return response.StatusCode switch
            {
                HttpStatusCode.OK or HttpStatusCode.NoContent => Result.Ok(),
                HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(id)),
                _ => Result.Fail($"Unknown error occured with status code {response.StatusCode}")
            };
        }
        catch (CosmosException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private NotFoundError CreateNotFoundError(Guid id)
    {
        return new NotFoundError($"ImageSet with id {id} not found");
    }
}