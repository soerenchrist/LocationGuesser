using FluentResults;
using LocationGuesser.Api.Contracts;
using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Api.Mappings;
using LocationGuesser.Api.Validators;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Tests.Features.ImageSets;

public class CreateImageSetTests
{
    private readonly CreateImageSetCommandHandler _cut;
    private readonly IImageSetRepository _repository = Substitute.For<IImageSetRepository>();
    public CreateImageSetTests()
    {
        var validator = new ImageSetValidator();
        _cut = new CreateImageSetCommandHandler(validator, _repository);
    }

    [Theory]
    [InlineData("", "Description", "Tags", 1900, 2000)]
    [InlineData("Title", "", "Tags", 1900, 2000)]
    [InlineData("Title", "Description", null, 1900, 2000)]
    [InlineData("Title", "Description", "Tags", 1900, 1890)]
    [InlineData("Title", "Description", "Tags", 1900, 1920)]
    public async Task Handler_ShouldReturnValidationErrorResult_WhenImageSetIsInvalid(string title,
        string description, string tags, int lower, int upper)
    {
        var command = new CreateImageSetCommand(title, description, tags, lower, upper);
        var result = await _cut.Handle(command, default);

        result.Errors.Any(x => x is ValidationError).Should().BeTrue();
    }

    [Fact]
    public async Task Handler_ShouldAddImageSetToRepository_WhenImageIsValid()
    {
        var command = new CreateImageSetCommand("Title", "Description", "Tags", 1900, 2000);
        _repository.AddImageSetAsync(Arg.Any<ImageSet>(), default)
            .ReturnsForAnyArgs(Task.FromResult(Result.Ok(command.ToDomain())));

        var result = await _cut.Handle(command, default);
        await _repository.Received().AddImageSetAsync(Arg.Is<ImageSet>(x =>
            x.Title == "Title" && x.Description == "Description" && x.LowerYearRange == 1900
            && x.UpperYearRange == 2000 && x.Tags == "Tags"), default);
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorResult_WhenAddingToRepoFails()
    {
        var command = new CreateImageSetCommand("Title", "Description", "Tags", 1900, 2000);
        _repository.AddImageSetAsync(Arg.Any<ImageSet>(), default)
            .ReturnsForAnyArgs(Task.FromResult(Result.Fail<ImageSet>("Something failed")));

        var result = await _cut.Handle(command, default);

        result.IsFailed.Should().BeTrue();
    }


    [Fact]
    public async Task Handle_ShouldReturnOkResultWithImageSet_WhenAddingToRepoSucceeds()
    {
        var command = new CreateImageSetCommand("Title", "Description", "Tags", 1900, 2000);
        _repository.AddImageSetAsync(Arg.Any<ImageSet>(), default)
            .ReturnsForAnyArgs(Task.FromResult(Result.Ok(command.ToDomain())));

        var result = await _cut.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<ImageSet>();
    }
}