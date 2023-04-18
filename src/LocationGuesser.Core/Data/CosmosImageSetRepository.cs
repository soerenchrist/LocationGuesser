using System.Net;
using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Data.Dtos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using LocationGuesser.Core.Domain.Errors;
using Azure.Identity;

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

            return item.Resource switch
            {
                null => Result.Fail(CreateNotFoundError(id)),
                CosmosImageSet image => Result.Ok(image.ToImageSet())
            };
        }
        catch (CosmosException ex)
        {
            return MatchExceptionToImageSetResult(id, ex);
        }
        catch (AuthenticationFailedException)
        {
            return Result.Fail<ImageSet>("Failed to authenticate with CosmosDB");
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
                FeedResponse<CosmosImageSet> response = await feed.ReadNextAsync(cancellationToken);
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
        catch (AuthenticationFailedException)
        {
            return Result.Fail("Failed to authenticate with CosmosDB");
        }
    }

    public async Task<Result<ImageSet>> AddImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
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
        catch (AuthenticationFailedException)
        {
            return Result.Fail("Failed to authenticate with CosmosDB");
        }

        return Result.Ok(imageSet);
    }

    public async Task<Result> UpdateImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.UpsertItemAsync(CosmosImageSet.FromImageSet(imageSet), cancellationToken);
            return Result.Ok();
        }
        catch (CosmosException ex)
        {
            return MatchExceptionToResult(imageSet.Id, ex);
        }
        catch (AuthenticationFailedException)
        {
            return Result.Fail("Failed to authenticate with CosmosDB");
        }
    }

    public async Task<Result> DeleteImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.DeleteItemAsync<CosmosImageSet>(id.ToString(),
                new PartitionKey(PartitionKey),
                cancellationToken: cancellationToken);
            return Result.Ok();
        }
        catch (CosmosException ex)
        {
            return MatchExceptionToResult(id, ex);
        }
        catch (AuthenticationFailedException)
        {
            return Result.Fail("Failed to authenticate with CosmosDB");
        }
    }

    private NotFoundError CreateNotFoundError(Guid id)
    {
        return new NotFoundError($"ImageSet with id {id} not found");
    }

    public Result<ImageSet> MatchExceptionToImageSetResult(Guid guid, CosmosException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(guid)),
            _ => Result.Fail(ex.Message)
        };
    }

    public Result MatchExceptionToResult(Guid guid, CosmosException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(guid)),
            _ => Result.Fail(ex.Message)
        };
    }
}