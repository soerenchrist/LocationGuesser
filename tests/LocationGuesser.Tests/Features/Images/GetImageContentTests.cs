using FluentResults;
using LocationGuesser.Api.Features.Images;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain.Errors;
using LocationGuesser.Tests.Utils;

namespace LocationGuesser.Tests.Features.Images;

public class GetImageContentQueryHandlerTests
{
    private readonly GetImageContentQueryHandler _cut;
    private readonly IBlobRepository _blobRepository = Substitute.For<IBlobRepository>();

    public GetImageContentQueryHandlerTests()
    {
        var logger = TestLogger.Create<GetImageContentQueryHandler>();
        var validator = new GetImageContentQueryValidator();
        _cut = new GetImageContentQueryHandler(_blobRepository, validator, logger);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_ShouldReturnValidationError_WhenNumberIsInvalid(int number)
    {
        var setId = Guid.NewGuid();

        var result = await _cut.Handle(new GetImageContentQuery(setId, number), default);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(x => x is ValidationError);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenBlobCallReturnsError()
    {
        var setId = Guid.NewGuid();
        var number = 1;
        var filename = $"{setId}/{number}.jpg";
        _blobRepository.DownloadImageAsync(filename, default)
            .Returns(Task.FromResult(Result.Fail<Stream>("Failed")));

        var result = await _cut.Handle(new GetImageContentQuery(setId, number), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnStream_WhenBlobCallSucceeds()
    {
        var setId = Guid.NewGuid();
        var number = 1;
        var stream = new MemoryStream();
        var filename = $"{setId}/{number}.jpg";
        _blobRepository.DownloadImageAsync(filename, default)
            .Returns(Task.FromResult(Result.Ok<Stream>(stream)));

        var result = await _cut.Handle(new GetImageContentQuery(setId, number), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(stream);
    }
}