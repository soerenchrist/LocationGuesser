using FluentResults;
using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Tests.Features.ImageSets;

public class GetImageSetTests
{
    private readonly GetImageSetQueryHandler _cut;
    private readonly IImageSetRepository _repository = Substitute.For<IImageSetRepository>();

    public GetImageSetTests()
    {
        _cut = new GetImageSetQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenRequestToRepositoryFails()
    {
        var guid = Guid.NewGuid();
        _repository.GetImageSetAsync(guid, default).Returns(Task.FromResult(Result.Fail<ImageSet>("Something failed")));

        var result = await _cut.Handle(new GetImageSetQuery(guid), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenRequestToRepositoryReturnsImageSet()
    {
        var guid = Guid.NewGuid();
        var imageSet = CreateImageSet(guid);
        _repository.GetImageSetAsync(guid, default).Returns(Task.FromResult(Result.Ok(imageSet)));

        var result = await _cut.Handle(new GetImageSetQuery(guid), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(imageSet);
    }

    private ImageSet CreateImageSet(Guid guid)
    {
        return new ImageSet(guid, "Title", "Description", "Tags", 1900, 2000, 10);
    }
}