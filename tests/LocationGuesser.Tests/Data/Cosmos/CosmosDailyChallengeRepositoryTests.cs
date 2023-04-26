using System.Net;
using Azure.Identity;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Data.Cosmos;
using LocationGuesser.Core.Data.Dtos;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Extensions;
using LocationGuesser.Tests.Utils;
using Microsoft.Azure.Cosmos;

namespace LocationGuesser.Tests.Data.Cosmos;

public class CosmosDailyChallengeRepositoryTests
{
    private readonly CosmosDailyChallengeRepository _cut;
    private readonly ICosmosDbContainer _container = Substitute.For<ICosmosDbContainer>();
    private readonly DateTime _dateTime = new(2023, 4, 1);

    public CosmosDailyChallengeRepositoryTests()
    {
        var logger = TestLogger.Create<CosmosDailyChallengeRepository>();
        _cut = new CosmosDailyChallengeRepository(_container, logger);
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnNotFound_WhenNoChallengeExistsYet()
    {
        _container.When(x => x
                .ReadItemAsync<CosmosDailyChallenge>(_dateTime.ToString("O"), new PartitionKey("DAILYCHALLENGES"),
                    default))
            .Throw(new CosmosException("Not found", HttpStatusCode.NotFound, 404, "", 10));

        var result = await _cut.GetDailyChallengeAsync(_dateTime, default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnFailed_WhenCosmosDbThrowsError()
    {
        _container.When(x => x
                .ReadItemAsync<CosmosDailyChallenge>(_dateTime.ToString("O"),
                    new PartitionKey("DAILYCHALLENGES"), default))
            .Throw(new CosmosException("Error", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.GetDailyChallengeAsync(_dateTime, default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeFalse();
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnFailed_WhenCosmosThrowsAuthException()
    {
        _container.When(x => x
                .ReadItemAsync<CosmosDailyChallenge>(_dateTime.ToString("O"),
                    new PartitionKey("DAILYCHALLENGES"), default))
            .Throw(new AuthenticationFailedException("Auth failed"));

        var result = await _cut.GetDailyChallengeAsync(_dateTime, default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeFalse();
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnOk_WhenCosmosReturnsObject()
    {
        var challenge = CreateDailyChallenge();
        var response = CreateResponse(challenge);
        _container.ReadItemAsync<CosmosDailyChallenge>(_dateTime.ToString("O"),
                new PartitionKey("DAILYCHALLENGES"), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.GetDailyChallengeAsync(_dateTime, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Should().Be("slug");
        result.Value.ImageNumbers.Should().BeEquivalentTo(new List<int>
        {
            1, 2, 3, 4
        });
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnNotFound_WhenCosmosReturnsNull()
    {
        var response = CreateResponse(null!);
        _container.ReadItemAsync<CosmosDailyChallenge>(_dateTime.ToString("O"),
                new PartitionKey("DAILYCHALLENGES"), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.GetDailyChallengeAsync(_dateTime, default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnFailed_WhenCosmosReturnsInvalidImages()
    {
        var challenge = CreateDailyChallenge("slug", "test,bla");
        var response = CreateResponse(challenge);
        _container.ReadItemAsync<CosmosDailyChallenge>(_dateTime.ToString("O"),
                new PartitionKey("DAILYCHALLENGES"), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.GetDailyChallengeAsync(_dateTime, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddDailyChallengeAsync_ShouldReturnFailed_WhenCosmosThrowsError()
    {
        var challenge = new DailyChallenge("slug", new List<int> { 1, 2, 3 });
        _container.When(x => x
                .CreateItemAsync(Arg.Is<CosmosDailyChallenge>(c =>
                    c.Date == DateTime.Today
                    && c.ImagesCsv == "1,2,3"
                    && c.SetSlug == "slug"), default))
            .Throw(new CosmosException("Error", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.AddDailyChallengeAsync(challenge, default);

        result.IsFailed.Should().BeTrue();
    }
    
    [Fact]
    public async Task AddDailyChallengeAsync_ShouldSetTimeToLive()
    {
        var challenge = new DailyChallenge("slug", new List<int> { 1, 2, 3 });
        _container.When(x => x
                .CreateItemAsync(Arg.Any<CosmosDailyChallenge>(), default))
            .Throw(new CosmosException("Error", HttpStatusCode.InternalServerError, 500, "", 10));

        await _cut.AddDailyChallengeAsync(challenge, default);

        await _container.Received(1)
            .CreateItemAsync(Arg.Is<CosmosDailyChallenge>(x => x.TimeToLive == 24 * 60 * 60), default);
    }

    [Fact]
    public async Task AddDailyChallengeAsync_ShouldReturnItem_WhenCallSucceeds()
    {
        
        var challenge = new DailyChallenge("slug", new List<int> { 1, 2, 3 });
        var response = CreateResponse(CreateDailyChallenge(challenge.Slug, "1,2,3"));
        _container.CreateItemAsync(Arg.Any<CosmosDailyChallenge>(), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.AddDailyChallengeAsync(challenge, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(challenge);
    }

    private ItemResponse<CosmosDailyChallenge> CreateResponse(CosmosDailyChallenge challenge)
    {
        var substitute = Substitute.For<ItemResponse<CosmosDailyChallenge>>();
        substitute.StatusCode.Returns(HttpStatusCode.OK);
        substitute.Resource.Returns(challenge);

        return substitute;
    }

    private CosmosDailyChallenge CreateDailyChallenge(string slug = "slug", string imagesCsv = "1,2,3,4")
    {
        return new CosmosDailyChallenge
        {
            Date = _dateTime,
            ImagesCsv = imagesCsv,
            SetSlug = slug
        };
    }
}