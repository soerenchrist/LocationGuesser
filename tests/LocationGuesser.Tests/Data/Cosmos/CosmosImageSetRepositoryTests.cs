using System.Net;
using Azure.Identity;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Data.Cosmos;
using LocationGuesser.Core.Data.Dtos;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using LocationGuesser.Tests.Utils;
using Microsoft.Azure.Cosmos;

namespace LocationGuesser.Tests.Data.Cosmos;

public class CosmosImageSetRepositoryTests
{
    private const string PartitionKey = "IMAGESETS";
    private readonly ICosmosDbContainer _container = Substitute.For<ICosmosDbContainer>();
    private readonly CosmosImageSetRepository _cut;

    public CosmosImageSetRepositoryTests()
    {
        var logger = TestLogger.Create<CosmosImageSetRepository>();
        _cut = new CosmosImageSetRepository(_container, logger);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnFail_WhenContainerReadThrowsException()
    {
        var id = Guid.NewGuid().ToString();
        _container.When(x => x.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "",
                10));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Something went wrong");
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnNotFoundError_WhenContainerReadThrowsExceptionWithNotFoundStatus()
    {
        var id = Guid.NewGuid().ToString();
        _container.When(x => x.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default))
            .Throw(new CosmosException("NotFound", HttpStatusCode.NotFound, 404, "",
                10));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnFail_WhenContainerReadThrowsAuthenticationFailed()
    {
        var id = Guid.NewGuid().ToString();
        _container.When(x => x.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default))
            .Throw(new AuthenticationFailedException("Failed"));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnNotFoundError_WhenStatusCodeIsNotFound()
    {
        var id = Guid.NewGuid().ToString();
        var taskResult = CreateResponse(HttpStatusCode.NotFound);
        _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default)
            .ReturnsForAnyArgs(Task.FromResult(taskResult));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnOkObject_WhenStatusCodeIsOkAndHasObject()
    {
        var id = Guid.NewGuid().ToString();
        var imageSet = CreateImageSet(id);
        var taskResult = CreateResponse(HttpStatusCode.OK, imageSet);
        _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default)
            .ReturnsForAnyArgs(Task.FromResult(taskResult));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(imageSet);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnNotFound_WhenStatusCodeIsOkButHasNoData()
    {
        var id = Guid.NewGuid().ToString();
        var taskResult = CreateResponse(HttpStatusCode.OK);
        _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default)
            .ReturnsForAnyArgs(Task.FromResult(taskResult));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnFail_WhenCosmosDbThrowsError()
    {
        _container.When(x => x.GetItemQueryIterator<CosmosImageSet>(Arg.Any<QueryDefinition>()))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "",
                10));

        var result = await _cut.ListImageSetsAsync(default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task ListImageSetsAsync_ReturnsEmptyList_WhenContainerReturnsEmptyFeed()
    {
        var emptyIterator = CreateFeedIterator(new List<CosmosImageSet>());
        _container.GetItemQueryIterator<CosmosImageSet>(Arg.Any<QueryDefinition>())
            .ReturnsForAnyArgs(emptyIterator);

        var result = await _cut.ListImageSetsAsync(default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListImageSetsAsync_ReturnsResults_WhenContainerReturnsFeed()
    {
        var imageSet1 = CreateImageSet(title: "Title1");
        var imageSet2 = CreateImageSet(title: "Title2");
        var iterator = CreateFeedIterator(new List<CosmosImageSet>
        {
            CosmosImageSet.FromImageSet(imageSet1),
            CosmosImageSet.FromImageSet(imageSet2)
        });
        _container.GetItemQueryIterator<CosmosImageSet>(Arg.Any<QueryDefinition>())
            .ReturnsForAnyArgs(iterator);

        var result = await _cut.ListImageSetsAsync(default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Should().BeEquivalentTo(imageSet1);
        result.Value[1].Should().BeEquivalentTo(imageSet2);
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnFail_WhenContainerReadThrowsAuthenticationFailed()
    {
        _container.When(x => x.GetItemQueryIterator<CosmosImageSet>(Arg.Any<QueryDefinition>()))
            .Throw(new AuthenticationFailedException("Failed"));

        var result = await _cut.ListImageSetsAsync(default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageSetAsync_ReturnsFail_WhenContainerThrowsException()
    {
        _container.When(x => x.CreateItemAsync(Arg.Any<CosmosImageSet>(), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.AddImageSetAsync(CreateImageSet(), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageSetAsync_ReturnsFail_WhenContainerReturnsInvalidStatusCode()
    {
        var actionResponse = CreateResponse(HttpStatusCode.InternalServerError);
        _container.CreateItemAsync(Arg.Any<CosmosImageSet>(), default)
            .ReturnsForAnyArgs(Task.FromResult(actionResponse));

        var result = await _cut.AddImageSetAsync(CreateImageSet(), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageSetAsync_ReturnsOk_WhenContainerReturnsValidStatusCode()
    {
        var imageSet = CreateImageSet();
        var actionResponse = CreateResponse(HttpStatusCode.Created, imageSet);
        _container.CreateItemAsync(Arg.Any<CosmosImageSet>(), default)
            .ReturnsForAnyArgs(Task.FromResult(actionResponse));

        var result = await _cut.AddImageSetAsync(imageSet, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageSetAsync_ShouldReturnFail_WhenContainerReadThrowsAuthenticationFailed()
    {
        var imageSet = CreateImageSet();
        _container.When(x => x.CreateItemAsync(Arg.Any<CosmosImageSet>(), default))
            .Throw(new AuthenticationFailedException("Failed"));

        var result = await _cut.AddImageSetAsync(imageSet, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateImageSetAsync_ReturnsFail_WhenContainerThrowsException()
    {
        _container.When(x => x.UpsertItemAsync(Arg.Any<CosmosImageSet>(), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.UpdateImageSetAsync(CreateImageSet(), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateImageSetAsync_ReturnsNotFound_WhenContainerThrowsExceptionWithNotFoundStatus()
    {
        _container.When(x => x.UpsertItemAsync(Arg.Any<CosmosImageSet>(), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.NotFound, 404, "", 10));

        var result = await _cut.UpdateImageSetAsync(CreateImageSet(), default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateImageSetAsync_ReturnsOk_WhenContainerReturnsSuccessStatus()
    {
        var cosmosResponse = CreateResponse(HttpStatusCode.OK, CreateImageSet());
        _container.UpsertItemAsync(Arg.Any<CosmosImageSet>(), default)
            .Returns(Task.FromResult(cosmosResponse));

        var result = await _cut.UpdateImageSetAsync(CreateImageSet(), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateImageSetAsync_ShouldReturnFail_WhenContainerReadThrowsAuthenticationFailed()
    {
        var imageSet = CreateImageSet();
        _container.When(x => x.UpsertItemAsync(Arg.Any<CosmosImageSet>(), default))
            .Throw(new AuthenticationFailedException("Failed"));

        var result = await _cut.UpdateImageSetAsync(imageSet, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageSetAsync_ReturnsFail_WhenContainerThrowsException()
    {
        _container.When(x => x.DeleteItemAsync<CosmosImageSet>(Arg.Any<string>(), Arg.Any<PartitionKey>(), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.DeleteImageSetAsync(Guid.NewGuid().ToString(), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageSetAsync_ReturnsNotFound_WhenContainerThrowsExceptionWithNotFoundStatus()
    {
        _container.When(x => x.DeleteItemAsync<CosmosImageSet>(Arg.Any<string>(), Arg.Any<PartitionKey>(), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.NotFound, 404, "", 10));

        var result = await _cut.DeleteImageSetAsync(Guid.NewGuid().ToString(), default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageSetAsync_ReturnsOk_WhenContainerReturnsOkStatus()
    {
        var response = CreateResponse(HttpStatusCode.OK);
        _container.DeleteItemAsync<CosmosImageSet>(Arg.Any<string>(), Arg.Any<PartitionKey>(), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.DeleteImageSetAsync(Guid.NewGuid().ToString(), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageSetAsync_ShouldReturnFail_WhenContainerReadThrowsAuthenticationFailed()
    {
        var imageSet = CreateImageSet();
        _container.When(x => x.DeleteItemAsync<CosmosImageSet>(Arg.Any<string>(), Arg.Any<PartitionKey>(), default))
            .Throw(new AuthenticationFailedException("Failed"));

        var result = await _cut.DeleteImageSetAsync(Guid.NewGuid().ToString(), default);

        result.IsFailed.Should().BeTrue();
    }

    private ItemResponse<CosmosImageSet> CreateResponse(HttpStatusCode statusCode, ImageSet? result = null)
    {
        var substitute = Substitute.For<ItemResponse<CosmosImageSet>>();
        substitute.StatusCode.Returns(statusCode);
        if (result != null) substitute.Resource.Returns(CosmosImageSet.FromImageSet(result));

        return substitute;
    }

    private FeedIterator<CosmosImageSet> CreateFeedIterator(List<CosmosImageSet> list)
    {
        var substitute = Substitute.For<FeedIterator<CosmosImageSet>>();
        if (list.Count == 0)
        {
            substitute.HasMoreResults.Returns(false);
        }
        else
        {
            var hasMoreItems = list.Skip(1).Select(_ => true).ToList();
            hasMoreItems.Add(false);
            var items = list.Skip(1).Select(x => CreateFeedResponse(x)).ToList();
            items.Add(null!);
            var first = CreateFeedResponse(list[0]);

            substitute.ReadNextAsync().Returns(first, items.ToArray());
            substitute.HasMoreResults.Returns(true, hasMoreItems.ToArray());
        }

        return substitute;
    }

    private FeedResponse<CosmosImageSet> CreateFeedResponse(CosmosImageSet imageSet)
    {
        var substitute = Substitute.For<FeedResponse<CosmosImageSet>>();
        substitute.GetEnumerator().Returns(new List<CosmosImageSet> { imageSet }.GetEnumerator());

        return substitute;
    }

    private ImageSet CreateImageSet(string setSlug = "", string title = "Title", string description = "Description")
    {
        return new ImageSet(setSlug, "Title", "Description", "Tags", 1900, 2000, 10);
    }
}