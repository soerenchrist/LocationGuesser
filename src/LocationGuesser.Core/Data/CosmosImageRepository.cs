using System.Net;
using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Data.Dtos;
using LocationGuesser.Core.Domain;
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
                _ => Result.Fail($"Unexpected status code {response.StatusCode}"),
            };
        }
        catch (CosmosException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> DeleteImageAsync(Image image, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.DeleteItemAsync<CosmosImage>(image.Number.ToString(), new PartitionKey(image.SetId.ToString()), cancellationToken);
            return response.StatusCode switch
            {
                HttpStatusCode.OK or HttpStatusCode.NoContent => Result.Ok(),
                _ => Result.Fail($"Unknown error with status code {response.StatusCode}"),
            };
        }
        catch (CosmosException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<Image?>> GetImageAsync(Guid setId, int number, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), cancellationToken);
            return (response.StatusCode, response.Resource) switch
            {
                (HttpStatusCode.NotFound, _) => Result.Ok<Image?>(null),
                (HttpStatusCode.OK, null) => Result.Ok<Image?>(null),
                (HttpStatusCode.OK, CosmosImage image) => Result.Ok<Image?>(image.ToImage()),
                _ => Result.Fail($"Unknown error with status code {response.StatusCode}"),
            };
        }
        catch (CosmosException ex)
        {
            return Result.Fail<Image?>(ex.Message);
        }
    }
}