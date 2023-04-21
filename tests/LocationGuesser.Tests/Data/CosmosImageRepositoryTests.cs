using System.Net;
using Azure.Identity;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Data.Dtos;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using LocationGuesser.Tests.Utils;
using Microsoft.Azure.Cosmos;

namespace LocationGuesser.Tests.Data;

public class CosmosImageRepositoryTests
{
    private readonly ICosmosDbContainer _container = Substitute.For<ICosmosDbContainer>();
    private readonly CosmosImageRepository _cut;

    public CosmosImageRepositoryTests()
    {
        _cut = new CosmosImageRepository(_container, TestLogger.Create<CosmosImageRepository>());
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnFail_WhenContainerThrowsException()
    {
        var setId = Guid.NewGuid().ToString();
        var number = 3;
        _container.When(x =>
                x.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnFail_WhenContainerThrowsAuthenticationException()
    {
        var setId = Guid.NewGuid().ToString();
        var number = 3;
        _container.When(x =>
                x.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default))
            .Throw(new AuthenticationFailedException("Failed"));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnNotFound_WhenContainerThrowsExceptionWithNotFoundStatus()
    {
        var setId = Guid.NewGuid().ToString();
        var number = 3;
        _container.When(x =>
                x.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default))
            .Throw(new CosmosException("Not found", HttpStatusCode.NotFound, 404, "", 10));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnFail_WhenCosmosReturnsInvalidStatusCode()
    {
        var setId = Guid.NewGuid().ToString();
        var number = 3;
        var response = CreateResponse(HttpStatusCode.InternalServerError);
        _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default)
            .ReturnsForAnyArgs(Task.FromResult(response));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnNotFoundError_WhenCosmosReturnsOkWithNullResult()
    {
        var setId = Guid.NewGuid().ToString();
        var number = 3;
        var response = CreateResponse(HttpStatusCode.OK);
        _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default)
            .ReturnsForAnyArgs(Task.FromResult(response));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeEquivalentTo(new NotFoundError($"Image {number} in {setId} not found"));
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnImage_WhenCosmosReturnsResult()
    {
        var setId = Guid.NewGuid().ToString();
        var number = 3;
        var image = new Image(Guid.NewGuid().ToString(), number, 1900, 49, 11, "", "", "");
        var response = CreateResponse(HttpStatusCode.OK, image);
        _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default)
            .ReturnsForAnyArgs(Task.FromResult(response));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(image);
    }

    [Fact]
    public async Task AddImageAsync_ShouldReturnFail_WhenCosmosThrowsException()
    {
        var image = CreateImage();
        _container.When(x => x.CreateItemAsync(Arg.Any<CosmosImage>(), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.AddImageAsync(image, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageAsync_ShouldReturnFail_WhenContainerThrowsAuthenticationException()
    {
        var image = CreateImage();
        _container.When(x => x.CreateItemAsync(Arg.Any<CosmosImage>(), default))
            .Throw(new AuthenticationFailedException("Failed"));

        var result = await _cut.AddImageAsync(image, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageAsync_ShouldReturnFail_WhenCosmosReturnsInvalidStatus()
    {
        var image = CreateImage();
        var response = CreateResponse(HttpStatusCode.InternalServerError);
        _container.CreateItemAsync(CosmosImage.FromImage(image), default)
            .ReturnsForAnyArgs(Task.FromResult(response));

        var result = await _cut.AddImageAsync(image, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task AddImageAsync_ShouldReturnOk_WhenCosmosReturnsCreated()
    {
        var image = CreateImage();
        var response = CreateResponse(HttpStatusCode.Created, image);
        _container.CreateItemAsync(CosmosImage.FromImage(image), default)
            .ReturnsForAnyArgs(Task.FromResult(response));

        var result = await _cut.AddImageAsync(image, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnOk_WhenCosmosReturnsOk()
    {
        var image = CreateImage();

        var response = CreateResponse(HttpStatusCode.OK, image);
        _container.DeleteItemAsync<CosmosImage>(image.Number.ToString(), new PartitionKey(image.SetSlug),
                default)
            .ReturnsForAnyArgs(Task.FromResult(response));

        var result = await _cut.DeleteImageAsync(image, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnFail_WhenCosmosThrowsException()
    {
        var image = CreateImage();

        _container.When(x =>
                x.DeleteItemAsync<CosmosImage>(image.Number.ToString(), new PartitionKey(image.SetSlug),
                    default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.DeleteImageAsync(image, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnNotFound_WhenCosmosThrowsExceptionWithNotFoundStatus()
    {
        var image = CreateImage();

        _container.When(x =>
                x.DeleteItemAsync<CosmosImage>(image.Number.ToString(), new PartitionKey(image.SetSlug),
                    default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.NotFound, 404, "", 10));

        var result = await _cut.DeleteImageAsync(image, default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnFail_WhenContainerThrowsAuthenticationException()
    {
        var image = CreateImage();
        _container.When(x => x.DeleteItemAsync<CosmosImage>(Arg.Any<string>(), Arg.Any<PartitionKey>(), default))
            .Throw(new AuthenticationFailedException("Failed"));

        var result = await _cut.DeleteImageAsync(image, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task ListImagesAsync_ShouldReturnFail_WhenContainerThrowsAuthenticationFailed()
    {
        _container.When(x => x.GetItemQueryIterator<CosmosImage>(Arg.Any<QueryDefinition>()))
            .Throw(new AuthenticationFailedException("Failed"));

        var result = await _cut.ListImagesAsync(Guid.NewGuid().ToString(), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task ListImagesAsync_ShouldReturnFail_WhenContainerThrowsCosmosException()
    {
        _container.When(x => x.GetItemQueryIterator<CosmosImage>(Arg.Any<QueryDefinition>()))
            .Throw(new CosmosException("Failed", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.ListImagesAsync(Guid.NewGuid().ToString(), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task ListImagesAsync_ShouldReturnEmptyList_WhenContainerReturnsEmptyFeed()
    {
        var emptyFeed = CreateFeedIterator(new List<CosmosImage>());
        _container.GetItemQueryIterator<CosmosImage>(Arg.Any<QueryDefinition>())
            .ReturnsForAnyArgs(emptyFeed);

        var results = await _cut.ListImagesAsync(Guid.NewGuid().ToString(), default);
        results.IsSuccess.Should().BeTrue();
        results.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListImagesAsync_ShouldReturnImages_WhenContainerReturnsFeed()
    {
        var image1 = CreateImage();
        var image2 = CreateImage();
        var image3 = CreateImage();
        var feed = CreateFeedIterator(new List<CosmosImage>
        {
            CosmosImage.FromImage(image1),
            CosmosImage.FromImage(image2),
            CosmosImage.FromImage(image3)
        });
        _container.GetItemQueryIterator<CosmosImage>(Arg.Any<QueryDefinition>())
            .Returns(feed);

        var results = await _cut.ListImagesAsync(Guid.NewGuid().ToString(), default);
        results.IsSuccess.Should().BeTrue();
        results.Value.Should().HaveCount(3);
        results.Value.Should().BeEquivalentTo(new List<Image>
        {
            image1, image2, image3
        });
    }

    private Image CreateImage()
    {
        return new Image("slug", 1, 2000, 49, 10, "Description", "License", "Url");
    }

    private ItemResponse<CosmosImage> CreateResponse(HttpStatusCode statusCode, Image? result = null)
    {
        var substitute = Substitute.For<ItemResponse<CosmosImage>>();
        substitute.StatusCode.Returns(statusCode);
        if (result != null) substitute.Resource.Returns(CosmosImage.FromImage(result));

        return substitute;
    }

    private FeedIterator<CosmosImage> CreateFeedIterator(List<CosmosImage> list)
    {
        var substitute = Substitute.For<FeedIterator<CosmosImage>>();
        if (list.Count == 0)
        {
            substitute.HasMoreResults.Returns(false);
        }
        else
        {
            var hasMoreItems = list.Skip(1).Select(_ => true).ToList();
            hasMoreItems.Add(false);
            var items = list.Skip(1).Select(CreateFeedResponse).ToList();
            items.Add(null!);
            var first = CreateFeedResponse(list[0]);

            substitute.ReadNextAsync().Returns(first, items.ToArray());
            substitute.HasMoreResults.Returns(true, hasMoreItems.ToArray());
        }

        return substitute;
    }

    private FeedResponse<CosmosImage> CreateFeedResponse(CosmosImage image)
    {
        var substitute = Substitute.For<FeedResponse<CosmosImage>>();
        substitute.GetEnumerator().Returns(new List<CosmosImage> { image }.GetEnumerator());

        return substitute;
    }
}