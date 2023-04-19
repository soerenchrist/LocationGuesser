using FluentResults;
using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Core.Data.Abstractions;

namespace LocationGuesser.Tests.Features.ImageSets;

public class DeleteImageSetTests
{
    private readonly DeleteImageSetCommandHandler _cut;
    private readonly IImageSetRepository _repository = Substitute.For<IImageSetRepository>();

    public DeleteImageSetTests()
    {
        _cut = new DeleteImageSetCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenRequestToRepositoryFails()
    {
        var guid = Guid.NewGuid();
        _repository.DeleteImageSetAsync(guid, default).Returns(Task.FromResult(Result.Fail("Something failed")));

        var result = await _cut.Handle(new DeleteImageSetCommand(guid), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenRequestToRepositoryReturnsOk()
    {
        var guid = Guid.NewGuid();
        _repository.DeleteImageSetAsync(guid, default).Returns(Task.FromResult(Result.Ok()));

        var result = await _cut.Handle(new DeleteImageSetCommand(guid), default);

        result.IsSuccess.Should().BeTrue();
    }
}