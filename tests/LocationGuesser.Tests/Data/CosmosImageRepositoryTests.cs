using System.Net;
using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Tests.Utils;
using Microsoft.Azure.Cosmos;
using LocationGuesser.Core.Data.Dtos;
using LocationGuesser.Core.Domain.Errors;

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
    public async Task GetImageAsync_ShouldReturnNotFoundError_WhenCosmosReturnsOkWithNullResult()
    {
        var setId = Guid.NewGuid();
        var number = 3;
        var response = CreateResponse(HttpStatusCode.OK);
        _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeEquivalentTo(new NotFoundError($"Image {number} in {setId} not found"));
    }

    [Fact]
    public async Task GetImageAsync_ShouldReturnNotFoundError_WhenCosmosReturnsNotFound()
    {
        var setId = Guid.NewGuid();
        var number = 3;
        var response = CreateResponse(HttpStatusCode.NotFound);
        _container.ReadItemAsync<CosmosImage>(number.ToString(), new PartitionKey(setId.ToString()), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.GetImageAsync(setId, number, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeEquivalentTo(new NotFoundError($"Image {number} in {setId} not found"));
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
        _container.DeleteItemAsync<CosmosImage>(image.Number.ToString(), new PartitionKey(image.SetId.ToString()), default)
            .ReturnsForAnyArgs(Task.FromResult(response));

        var result = await _cut.DeleteImageAsync(image, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnFail_WhenCosmosThrowsException()
    {
        var image = CreateImage();

        _container.When(x => x.DeleteItemAsync<CosmosImage>(image.Number.ToString(), new PartitionKey(image.SetId.ToString()), default))
            .Throw(new CosmosException("Something went wrong", HttpStatusCode.InternalServerError, 500, "", 10));

        var result = await _cut.DeleteImageAsync(image, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnNotFound_WhenCosmosReturnsNotFound()
    {
        var image = CreateImage();
        
        var response = CreateResponse(HttpStatusCode.NotFound);
        _container.DeleteItemAsync<CosmosImage>(image.Number.ToString(), new PartitionKey(image.SetId.ToString()), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.DeleteImageAsync(image, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeEquivalentTo(new NotFoundError($"Image {image.Number} in {image.SetId} not found"));
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnFail_WhenCosmosReturnsInvalidStatusCode()
    {
        var image = CreateImage();
        var response = CreateResponse(HttpStatusCode.InternalServerError);

        _container.DeleteItemAsync<CosmosImage>(image.Number.ToString(), new PartitionKey(image.SetId.ToString()), default)
            .Returns(Task.FromResult(response));

        var result = await _cut.DeleteImageAsync(image, default);

        result.IsFailed.Should().BeTrue();
    }

    private Image CreateImage()
    {
        return new Image(Guid.NewGuid(), 1, 2000, 49, 10, "Description", "License");
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