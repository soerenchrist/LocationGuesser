using System.Net;
using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Tests.Utils;
using Microsoft.Azure.Cosmos;

namespace LocationGuesser.Tests.Data;

public class CosmosImageSetRepositoryTests
{
    private readonly CosmosImageSetRepository _cut;
    private readonly ICosmosDbContainer _container = Substitute.For<ICosmosDbContainer>();
    private const string PartitionKey = "IMAGESETS";

    public CosmosImageSetRepositoryTests()
    {
        var logger = TestLogger.Create<CosmosImageSetRepository>();
        _cut = new CosmosImageSetRepository(_container, logger);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnFail_WhenContainerReadThrowsException()
    {
        var id = Guid.NewGuid();
        _container.When(x => x.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default))
            .Throw(new CosmosException("Something went wrong", System.Net.HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Something went wrong");
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnNullResult_WhenStatusCodeIsNotFound()
    {
        var id = Guid.NewGuid();
        var taskResult = CreateResponse(HttpStatusCode.NotFound);
        _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default)
            .ReturnsForAnyArgs(Task.FromResult(taskResult));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnFail_WhenStatusCodeIsOtherThanNotFoundOrOk()
    {

        var id = Guid.NewGuid();
        var taskResult = CreateResponse(HttpStatusCode.InternalServerError);
        _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default)
            .ReturnsForAnyArgs(Task.FromResult(taskResult));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnOkObject_WhenStatusCodeIsOkAndHasObject()
    {

        var id = Guid.NewGuid();
        var imageSet = new ImageSet(id, "Title", "Description", "Tags", 1900, 2000);
        var taskResult = CreateResponse(HttpStatusCode.OK, imageSet);
        _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default)
            .ReturnsForAnyArgs(Task.FromResult(taskResult));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(imageSet);
    }

    [Fact]
    public async Task GetImageSetAsync_ShouldReturnNull_WhenStatusCodeIsOkButHasNoData()
    {

        var id = Guid.NewGuid();
        var taskResult = CreateResponse(HttpStatusCode.OK);
        _container.ReadItemAsync<CosmosImageSet>(id.ToString(), new PartitionKey(PartitionKey), default)
            .ReturnsForAnyArgs(Task.FromResult(taskResult));

        var result = await _cut.GetImageSetAsync(id, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ListImageSetsAsync_ShouldReturnFail_WhenCosmosDbThrowsError()
    {
        _container.When(x => x.GetItemQueryIterator<CosmosImageSet>(Arg.Any<QueryDefinition>()))
            .Throw(new CosmosException("Something went wrong", System.Net.HttpStatusCode.InternalServerError, 500, "", 10));

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
        var imageSet1 = new ImageSet(Guid.NewGuid(), "Title1", "description1", "Tags1", 1900, 2000);
        var imageSet2 = new ImageSet(Guid.NewGuid(), "Title2", "description2", "Tags2", 1900, 2000);
        var iterator = CreateFeedIterator(new List<CosmosImageSet>{
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
    public async Task AddImageSetAsync_ReturnsFail_WhenContainerThrowsException()
    {
        _container.When(x => x.CreateItemAsync<CosmosImageSet>(Arg.Any<CosmosImageSet>(), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.AddImageSetAsync(new ImageSet(Guid.Empty, "", "", "", 1900, 2000), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageSetAsync_ReturnsFail_WhenContainerReturnsInvalidStatusCode()
    {
        var actionResponse = CreateResponse(HttpStatusCode.InternalServerError);
        _container.CreateItemAsync<CosmosImageSet>(Arg.Any<CosmosImageSet>(), cancellationToken: default)
            .ReturnsForAnyArgs(Task.FromResult(actionResponse));

        var result = await _cut.AddImageSetAsync(new ImageSet(Guid.Empty, "", "", "", 1900, 2000), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageSetAsync_ReturnsOk_WhenContainerReturnsValidStatusCode()
    {
        var actionResponse = CreateResponse(HttpStatusCode.Created, new ImageSet(Guid.NewGuid(), "", "", "", 1900, 2000));
        _container.CreateItemAsync<CosmosImageSet>(Arg.Any<CosmosImageSet>(), cancellationToken: default)
            .ReturnsForAnyArgs(Task.FromResult(actionResponse));

        var result = await _cut.AddImageSetAsync(new ImageSet(Guid.Empty, "", "", "", 1900, 2000), default);

        result.IsSuccess.Should().BeTrue();
    }

    private ItemResponse<CosmosImageSet> CreateResponse(HttpStatusCode statusCode, ImageSet? result = null)
    {
        var substitute = Substitute.For<ItemResponse<CosmosImageSet>>();
        substitute.StatusCode.Returns(statusCode);
        if (result != null)
        {
            substitute.Resource.Returns(CosmosImageSet.FromImageSet(result));
        }
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

            substitute.ReadNextAsync(default).Returns(first, items.ToArray());
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
}