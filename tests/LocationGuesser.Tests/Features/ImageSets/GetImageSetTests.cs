using FluentResults;
using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using LocationGuesser.Tests.Utils;

namespace LocationGuesser.Tests.Features.ImageSets;

public class GetImageSetQueryHandlerTests
{
    private readonly GetImageSetQueryHandler _cut;
    private readonly IImageSetRepository _imageSetRepository = Substitute.For<IImageSetRepository>();
    public GetImageSetQueryHandlerTests()
    {
        var logger = TestLogger.Create<GetImageSetQueryHandler>();
        _cut = new GetImageSetQueryHandler(_imageSetRepository, logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenImageSetDoesNotExist()
    {
        var slug = "non-existing-set";
        _imageSetRepository.GetImageSetAsync(slug, CancellationToken.None)
            .Returns(Task.FromResult(Result.Fail<ImageSet>(new NotFoundError("Not found"))));
        var query = new GetImageSetQuery(slug);
        var result = await _cut.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(x => x is NotFoundError);
    }

    [Fact]
    public async Task Handle_ShouldReturnImageSet_WhenImageSetExists()
    {
        var slug = "existing-set";
        _imageSetRepository.GetImageSetAsync(slug, CancellationToken.None)
            .Returns(Task.FromResult(Result.Ok(CreateImageSet(slug))));
        var query = new GetImageSetQuery(slug);
        var result = await _cut.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    private ImageSet CreateImageSet(string slug)
    {
        return new ImageSet(slug, "Title", "Descritpion", "Tags", 1900, 2000, 10);
    }
}