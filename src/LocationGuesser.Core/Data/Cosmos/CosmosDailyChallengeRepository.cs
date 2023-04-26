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

public class CosmosDailyChallengeRepository : IDailyChallengeRepository
{
    private readonly ICosmosDbContainer _container;
    private readonly ILogger<CosmosDailyChallengeRepository> _logger;

    public CosmosDailyChallengeRepository(ICosmosDbContainer container, ILogger<CosmosDailyChallengeRepository> logger)
    {
        _container = container;
        _logger = logger;
    }

    public async Task<Result<DailyChallenge>> GetDailyChallengeAsync(DateTime date, CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey("DAILYCHALLENGES");
        try
        {
            var response =
                await _container.ReadItemAsync<CosmosDailyChallenge>(date.ToString("O"), partitionKey,
                    cancellationToken);
            return ParseResponse(response);
        }
        catch (CosmosException ex)
        {
            return ParseException(date, ex);
        }
        catch (AuthenticationFailedException ex)
        {
            return ParseException(date, ex);
        }
    }

    public async Task<Result<DailyChallenge>> AddDailyChallengeAsync(DailyChallenge dailyChallenge,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = new CosmosDailyChallenge
            {
                Date = DateTime.Today,
                ImagesCsv = string.Join(',', dailyChallenge.ImageNumbers),
                SetSlug = dailyChallenge.Slug
            };
            await _container.CreateItemAsync(item, cancellationToken);
            return Result.Ok(dailyChallenge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding daily challenge to Cosmos DB");
            return Result.Fail<DailyChallenge>("Error while adding daily challenge to Cosmos DB");
        }
    }

    private Result<DailyChallenge> ParseException(DateTime date, Exception ex)
    {
        switch (ex)
        {
            case CosmosException cosmosException:
            {
                _logger.LogError("Error while reading daily challenge from Cosmos DB: {Message}", ex.Message);
                if (cosmosException.StatusCode == HttpStatusCode.NotFound)
                {
                    return Result.Fail<DailyChallenge>(new NotFoundError($"No daily challenge found for date {date}"));
                }

                return Result.Fail<DailyChallenge>("Error while reading daily challenge from Cosmos DB");
            }
            case AuthenticationFailedException:
                return Result.Fail<DailyChallenge>("Failed to authenticate with Cosmos DB");
            default:
                return Result.Fail<DailyChallenge>("Unexpected error while reading daily challenge from Cosmos DB");
        }
    }

    private Result<DailyChallenge> ParseResponse(ItemResponse<CosmosDailyChallenge> response)
    {
        if (response.Resource == null) return Result.Fail(new NotFoundError("No daily challenge found"));

        var imagesCsv = response.Resource.ImagesCsv;
        try
        {
            var images = imagesCsv.Split(',').Select(int.Parse).ToList();

            var challenge = new DailyChallenge(response.Resource.SetSlug, images);
            return Result.Ok(challenge);
        }
        catch (FormatException)
        {
            return Result.Fail("Error while parsing daily challenge from Cosmos DB");
        }
    }
}