using System.Net;
using Azure.Identity;
using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Data.Dtos;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LocationGuesser.Core.Data;

public class CosmosImageSetRepository : IImageSetRepository
{
    private const string PartitionKey = "IMAGESETS";
    private readonly ICosmosDbContainer _container;
    private readonly ILogger<CosmosImageSetRepository> _logger;

    public CosmosImageSetRepository(ICosmosDbContainer container,
        ILogger<CosmosImageSetRepository> logger)
    {
        _container = container;
        _logger = logger;
    }

    public async Task<Result<ImageSet>> GetImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting image set with id {id}", id);
        try
        {
            var item = await _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey),
                cancellationToken);

            return item.Resource switch
            {
                null => Result.Fail(CreateNotFoundError(id)),
                var image => Result.Ok(image.ToImageSet())
            };
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to get image set with id {id}", id);
            return MatchExceptionToImageSetResult(id, ex);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Failed to authenticate against CosmosDB");
            return Result.Fail<ImageSet>("Failed to authenticate with CosmosDB");
        }
    }


    public async Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Listing image sets");
        try
        {
            var query = new QueryDefinition("SELECT * FROM c where c.type = 'ImageSet'");

            using var feed = _container.GetItemQueryIterator<CosmosImageSet>(
                query
            );
            var items = new List<ImageSet>();
            while (feed.HasMoreResults)
            {
                var response = await feed.ReadNextAsync(cancellationToken);
                foreach (var item in response) items.Add(item.ToImageSet());
            }

            _logger.LogDebug("Found {count} image sets", items.Count);
            return Result.Ok(items);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to list image sets");
            return Result.Fail(ex.Message);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Failed to authenticate against CosmosDB");
            return Result.Fail("Failed to authenticate with CosmosDB");
        }
    }

    public async Task<Result<ImageSet>> AddImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Adding image set {imageSet}", imageSet);
        try
        {
            var response = await _container.CreateItemAsync(CosmosImageSet.FromImageSet(imageSet),
                cancellationToken);
            if (response.StatusCode != HttpStatusCode.Created)
                return Result.Fail($"Failed to create image set with status code {response.StatusCode}");
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to add image set {ImageSet}", imageSet);
            return Result.Fail(ex.Message);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Failed to authenticate against CosmosDB");
            return Result.Fail("Failed to authenticate with CosmosDB");
        }

        return Result.Ok(imageSet);
    }

    public async Task<Result> UpdateImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating image set {imageSet}", imageSet);
        try
        {
            await _container.UpsertItemAsync(CosmosImageSet.FromImageSet(imageSet), cancellationToken);
            return Result.Ok();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to update image set {ImageSet}", imageSet);
            return MatchExceptionToResult(imageSet.Id, ex);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Failed to authenticate against CosmosDB");
            return Result.Fail("Failed to authenticate with CosmosDB");
        }
    }

    public async Task<Result> DeleteImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting image set with id {id}", id);
        try
        {
            await _container.DeleteItemAsync<CosmosImageSet>(id.ToString(),
                new PartitionKey(PartitionKey),
                cancellationToken);
            return Result.Ok();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to delete image set with id {id}", id);
            return MatchExceptionToResult(id, ex);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Failed to authenticate against CosmosDB");
            return Result.Fail("Failed to authenticate with CosmosDB");
        }
    }

    private NotFoundError CreateNotFoundError(Guid id)
    {
        _logger.LogInformation("ImageSet with id {id} not found", id);
        return new NotFoundError($"ImageSet with id {id} not found");
    }

    private Result<ImageSet> MatchExceptionToImageSetResult(Guid guid, CosmosException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(guid)),
            _ => Result.Fail(ex.Message)
        };
    }

    private Result MatchExceptionToResult(Guid guid, CosmosException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(guid)),
            _ => Result.Fail(ex.Message)
        };
    }
}