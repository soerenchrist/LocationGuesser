using Azure;
using Azure.Storage.Blobs.Models;
using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;

namespace LocationGuesser.Tests.Data;

public class BlobRepositoryTests
{
    private readonly BlobRepository _cut;
    private readonly IBlobContainer _container = Substitute.For<IBlobContainer>();

    public BlobRepositoryTests()
    {
        _cut = new BlobRepository(_container);
    }

    [Fact]
    public async Task UploadImageAsync_ShouldReturnFail_WhenBlobCallThrowsException()
    {
        var filename = "test.png";
        _container.When(x => x.UploadAsync(filename, Stream.Null, default, true))
            .Throw(new RequestFailedException("Something went wrong"));

        var result = await _cut.UploadImageAsync(filename, Stream.Null, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task UploadImageAsync_ShouldReturnOk_WhenBlobCallSucceeds()
    {
        var filename = "test.png";
        var contentInfo = CreateContentInfo();
        _container.UploadAsync(filename, Stream.Null, default, true)
            .Returns(Task.FromResult(contentInfo));

        var result = await _cut.UploadImageAsync(filename, Stream.Null, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnFail_WhenBlobCallThrowsException()
    {
        var filename = "test.png";
        _container.When(x => x.DeleteAsync(filename, default))
            .Throw(new RequestFailedException("Something went wrong"));

        var result = await _cut.DeleteImageAsync(filename, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldReturnOk_WhenBlobCallSucceeds()
    {
        var filename = "test.png";
        _container.DeleteAsync(filename, default)
            .Returns(Task.CompletedTask);

        var result = await _cut.DeleteImageAsync(filename, default);

        result.IsSuccess.Should().BeTrue();
    }

    private BlobContentInfo CreateContentInfo()
    {
        var contentInfo = Substitute.For<BlobContentInfo>();
        return contentInfo;
    }
}