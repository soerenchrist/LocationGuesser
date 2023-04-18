using FluentResults;
using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Tests.Features.ImageSets;

public class ListImageSetsTests
{
    private readonly ListImageSetsQueryHandler _cut;
    private readonly IImageSetRepository _imageSetRepository = Substitute.For<IImageSetRepository>();

    public ListImageSetsTests()
    {
        _cut = new ListImageSetsQueryHandler(_imageSetRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenRepositoryReturnsFail()
    {
        _imageSetRepository.ListImageSetsAsync(default)
            .Returns(Task.FromResult(Result.Fail<List<ImageSet>>("Error")));

        var result = await _cut.Handle(new ListImageSetsQuery(), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRepositoryReturnsSuccess()
    {
        _imageSetRepository.ListImageSetsAsync(default)
            .Returns(Task.FromResult(Result.Ok(new List<ImageSet>())));

        var result = await _cut.Handle(new ListImageSetsQuery(), default);

        result.IsSuccess.Should().BeTrue();
    }
}