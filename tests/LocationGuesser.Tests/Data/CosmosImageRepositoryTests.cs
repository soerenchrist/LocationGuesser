using System.Net;
using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
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
        var setId = Guid.NewGuid();
        var number = 3;
        _container.When(x => x.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnFail_WhenCosmosReturnsInvalidStatusCode()
    {
        var setId = Guid.NewGuid();
        var number = 3;
        var response = CreateResponse(HttpStatusCode.InternalServerError);
        _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnNull_WhenCosmosReturnsOkWithNullResult()
    {
        var setId = Guid.NewGuid();
        var number = 3;
        var response = CreateResponse(HttpStatusCode.OK);
        _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnNull_WhenCosmosReturnsNotFound()
    {
        var setId = Guid.NewGuid();
        var number = 3;
        var response = CreateResponse(HttpStatusCode.NotFound);
        _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnImage_WhenCosmosReturnsResult()
    {
        var setId = Guid.NewGuid();
        var number = 3;
        var image = new Image(Guid.NewGuid(), number, 1900, 49, 11, "", "");
        var response = CreateResponse(HttpStatusCode.OK, image);
        _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(image);
    }

    private ItemResponse<CosmosImage> CreateResponse(HttpStatusCode statusCode, Image? result = null)
    {
        var substitute = Substitute.For<ItemResponse<CosmosImage>>();
        substitute.StatusCode.Returns(statusCode);
        if (result != null)
        {
            substitute.Resource.Returns(CosmosImage.FromImage(result));
        }
        return substitute;
    }
}