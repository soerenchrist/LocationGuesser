using FluentResults;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Api.Features.Game;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Tests.Features.Game;

public class GetGameQueryHandlerTests
{
    private readonly GetGameQueryHandler _cut;
    private readonly IImageService _imageService = Substitute.For<IImageService>();

    public GetGameQueryHandlerTests()
    {
        var validator = new GetGameQueryValidator();
        _cut = new GetGameQueryHandler(_imageService, validator);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenServiceReturnsNotFound()
    {
        var setId = Guid.NewGuid();
        var imageCount = 10;
        _imageService.GetGameSetAsync(setId, imageCount, default)
            .Returns(Task.FromResult(Result.Fail<List<Image>>(new NotFoundError("Set not found"))));

        var result = await _cut.Handle(new GetGameQuery(setId, imageCount), default);

        result.IsFailed.Should().BeTrue();
        result.IsNotFound().Should().BeTrue();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public async Task Handle_ShouldReturnValidationError_WhenRequestIsInvalid(int imageCount)
    {
        var setId = Guid.NewGuid();

        var result = await _cut.Handle(new GetGameQuery(setId, imageCount), default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<ValidationError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnIMages_WhenRequestIsValid()
    {
        var setId = Guid.NewGuid();
        var imageCount = 10;
        var images = CreateImages(setId, imageCount);
        _imageService.GetGameSetAsync(setId, imageCount, default)
            .Returns(Task.FromResult(Result.Ok(images)));

        var result = await _cut.Handle(new GetGameQuery(setId, imageCount), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(images);
    }

    public List<Image> CreateImages(Guid setId, int count)
    {
        return Enumerable.Range(1, count).Select(x => new Image(setId, x, 2020, 0, 0, $"Description {x}", "License")).ToList();
    }
}