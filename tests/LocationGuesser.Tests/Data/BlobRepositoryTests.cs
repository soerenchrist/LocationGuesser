using Azure;
using Azure.Storage.Blobs.Models;
using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Tests.Data;

public class BlobRepositoryTests
{
    private readonly IBlobContainer _container = Substitute.For<IBlobContainer>();
    private readonly BlobRepository _cut;

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

    [Fact]
    public async Task DownloadImageAsync_ShouldReturnStream_WhenContainerReturnsStream()
    {
        var filename = "test.png";
        var stream = new MemoryStream();
        _container.DownloadContentAsync(filename, default)
            .Returns(Task.FromResult<Stream?>(stream));

        var result = await _cut.DownloadImageAsync(filename, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(stream);
    }

    [Fact]
    public async Task DownloadImageAsync_ShouldReturnNotFoudn_WhenContainerReturnsNull()
    {
        var filename = "test.png";
        _container.DownloadContentAsync(filename, default)
            .Returns(Task.FromResult<Stream?>(null));

        var result = await _cut.DownloadImageAsync(filename, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(x => x is NotFoundError);
    }

    [Fact]
    public async Task DownloadImageAsync_ShouldReturnError_WhenContainerThrowsException()
    {
        var filename = "test.png";
        _container.When(x => x.DownloadContentAsync(filename, default))
            .Throw(new RequestFailedException("Something went wrong"));

        var result = await _cut.DownloadImageAsync(filename, default);

        result.IsFailed.Should().BeTrue();
    }

    private BlobContentInfo CreateContentInfo()
    {
        var contentInfo = Substitute.For<BlobContentInfo>();
        return contentInfo;
    }
}