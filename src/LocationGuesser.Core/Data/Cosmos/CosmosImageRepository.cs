using System.Net;
using Azure.Identity;
using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Data.Dtos;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LocationGuesser.Core.Data.Cosmos;

public class CosmosImageRepository : IImageRepository
{
    private readonly ICosmosDbContainer _container;
    private readonly ILogger<CosmosImageRepository> _logger;

    public CosmosImageRepository(ICosmosDbContainer container,
        ILogger<CosmosImageRepository> logger)
    {
        _container = container;
        _logger = logger;
    }

    public async Task<Result> AddImageAsync(Image image, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Adding image with number {number} to set {setId}", image.Number, image.SetSlug);
        try
        {
            var response = await _container.CreateItemAsync(CosmosImage.FromImage(image), cancellationToken);
            return response.StatusCode switch
            {
                HttpStatusCode.Created or HttpStatusCode.OK => Result.Ok(),
                _ => Result.Fail($"Unexpected status code {response.StatusCode}")
            };
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to add image with number {number} to set {setId}", image.Number, image.SetSlug);
            return Result.Fail(ex.Message);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Failed to authenticate against CosmosDB");
            return Result.Fail("Failed to authenticate against CosmosDB");
        }
    }

    public async Task<Result> DeleteImageAsync(Image image, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting image with number {number} from set {setId}", image.Number, image.SetSlug);
        try
        {
            await _container.DeleteItemAsync<CosmosImage>(image.Number.ToString(),
                new PartitionKey(image.SetSlug), cancellationToken);
            return Result.Ok();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to delete image with number {number} from set {setId}", image.Number,
                image.SetSlug);
            return MatchExceptionToResult(image.SetSlug, image.Number, ex);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Failed to authenticate against CosmosDB");
            return Result.Fail("Failed to authenticate against CosmosDB");
        }
    }

    public async Task<Result<List<Image>>> ListImagesAsync(string setSlug, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Listing images in set {setId}", setSlug);
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.setSlug = @setSlug and c.type = 'Image'")
                .WithParameter("@setSlug", setSlug);
            var feed = _container.GetItemQueryIterator<CosmosImage>(query);
            var items = new List<Image>();
            while (feed.HasMoreResults)
            {
                var response = await feed.ReadNextAsync(cancellationToken);
                foreach (var item in response) items.Add(item.ToImage());
            }

            return Result.Ok(items);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to list images in set {setId}", setSlug);
            return Result.Fail<List<Image>>("Unknown error with status code " + ex.StatusCode);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Failed to authenticate against CosmosDB");
            return Result.Fail<List<Image>>("Failed to authenticate against CosmosDB");
        }

        throw new NotImplementedException();
    }

    public async Task<Result<Image>> GetImageAsync(string setSlug, int number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting image with number {number} from set {setId}", number, setSlug);
        try
        {
            var response = await _container.ReadItemAsync<CosmosImage>(number.ToString(),
                new PartitionKey(setSlug), cancellationToken);
            return response.Resource switch
            {
                null => Result.Fail(CreateNotFoundError(setSlug, number)),
                var image => Result.Ok(image.ToImage())
            };
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to get image with number {number} from set {setId}", number, setSlug);
            return MatchExceptionToImageResult(setSlug, number, ex);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Failed to authenticate against CosmosDB");
            return Result.Fail("Failed to authenticate against CosmosDB");
        }
    }

    private Result<Image> MatchExceptionToImageResult(string setSlug, int number, CosmosException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(setSlug, number)),
            _ => Result.Fail<Image>(ex.Message)
        };
    }

    private Result MatchExceptionToResult(string setSlug, int number, CosmosException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(setSlug, number)),
            _ => Result.Fail(ex.Message)
        };
    }

    private NotFoundError CreateNotFoundError(string setSlug, int number)
    {
        _logger.LogInformation("Image {number} in {setId} not found", number, setSlug);
        return new NotFoundError($"Image {number} in {setSlug} not found");
    }
}