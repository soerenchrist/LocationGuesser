using FluentResults;
using LocationGuesser.Api.Features.Images;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Tests.Features.Images;

public class CreateImageCommandHandlerTests
{
    private readonly CreateImageCommandHandler _cut;
    private readonly IImageService _imageService = Substitute.For<IImageService>();

    public CreateImageCommandHandlerTests()
    {
        var validator = new CreateImageCommandValidator();
        _cut = new CreateImageCommandHandler(_imageService, validator);
    }

    [Theory]
    [InlineData("", 2020, 0, 0, "")]
    [InlineData("Description", -2020, 0, 0, "")]
    [InlineData("Description", 2020, -100, 0, "")]
    [InlineData("Description", 2020, 100, 0, "")]
    [InlineData("Description", 2020, 0, -200, "")]
    [InlineData("Description", 2020, 0, 200, "")]
    [InlineData("Description", 2020, 0, 0, null)]
    public async Task Handle_ShouldReturnInvalidResult_WhenCommandIsInvalid(string description, int year, double latitude, double longitude, string license)
    {
        var command = new CreateImageCommand(Guid.NewGuid(), description, year, latitude, longitude, license);
        var result = await _cut.Handle(command, default);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<ValidationError>();
    }

    [Fact]
    public async Task Handle_ShouldCreateImageInService_WhenCommandIsValid()
    {
        var setId = Guid.NewGuid();

        var command = new CreateImageCommand(setId, "Description", 2020, 0, 0, "License");
        _imageService.AddImageToImageSetAsync(setId, Arg.Any<Image>(), Arg.Any<Stream>(), default)
            .Returns(Task.FromResult(Result.Ok()));
        var result = await _cut.Handle(command, default);

        await _imageService.Received(1).AddImageToImageSetAsync(setId, Arg.Is<Image>(x =>
            x.Description == "Description"
            && x.Year == 2020
            && x.Latitude == 0
            && x.Longitude == 0
            && x.License == "License"), Arg.Any<Stream>(), default);
        result.IsSuccess.Should().BeTrue();
    }
}