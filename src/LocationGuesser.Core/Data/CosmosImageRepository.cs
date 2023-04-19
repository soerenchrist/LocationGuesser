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
            return Result.Fail(ex.Message);
        }
        catch (AuthenticationFailedException)
        {
            return Result.Fail("Failed to authenticate against CosmosDB");
        }
    }

    public async Task<Result> DeleteImageAsync(Image image, CancellationToken cancellationToken)
    {
        try
        {
            await _container.DeleteItemAsync<CosmosImage>(image.Number.ToString(),
                new PartitionKey(image.SetId.ToString()), cancellationToken);
            return Result.Ok();
        }
        catch (CosmosException ex)
        {
            return MatchExceptionToResult(image.SetId, image.Number, ex);
        }
        catch (AuthenticationFailedException)
        {
            return Result.Fail("Failed to authenticate against CosmosDB");
        }
    }

    public async Task<Result<List<Image>>> ListImagesAsync(Guid setId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.setId = @setId and c.type = 'Image'")
                .WithParameter("@setId", setId.ToString());
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
            return Result.Fail<List<Image>>("Unknown error with status code " + ex.StatusCode);
        }
        catch (AuthenticationFailedException)
        {
            return Result.Fail<List<Image>>("Failed to authenticate against CosmosDB");
        }

        throw new NotImplementedException();
    }

    public async Task<Result<Image>> GetImageAsync(Guid setId, int number, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<CosmosImage>(number.ToString(),
                new PartitionKey(setId.ToString()), cancellationToken);
            return response.Resource switch
            {
                null => Result.Fail(CreateNotFoundError(setId, number)),
                var image => Result.Ok(image.ToImage())
            };
        }
        catch (CosmosException ex)
        {
            return MatchExceptionToImageResult(setId, number, ex);
        }
        catch (AuthenticationFailedException)
        {
            return Result.Fail("Failed to authenticate against CosmosDB");
        }
    }

    private Result<Image> MatchExceptionToImageResult(Guid setId, int number, CosmosException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(setId, number)),
            _ => Result.Fail<Image>(ex.Message)
        };
    }

    private Result MatchExceptionToResult(Guid setId, int number, CosmosException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail(CreateNotFoundError(setId, number)),
            _ => Result.Fail(ex.Message)
        };
    }

    private NotFoundError CreateNotFoundError(Guid setId, int number)
    {
        return new NotFoundError($"Image {number} in {setId} not found");
    }
}