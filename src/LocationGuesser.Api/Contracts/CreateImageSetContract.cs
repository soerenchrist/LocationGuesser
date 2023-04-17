using FluentValidation;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Api.Contracts;

public class CreateImageSetContract
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Tags { get; init; }
    public required int LowerYearRange { get; init; }
    public required int UpperYearRange { get; init; }

    public ImageSet ToImageSet()
    {
        return new ImageSet(Guid.NewGuid(), Title, Description, Tags, LowerYearRange, UpperYearRange, 0);
    }
}

public class CreateImageSetContractValidator : AbstractValidator<CreateImageSetContract>
{
    public CreateImageSetContractValidator()
    {
        RuleFor(x => x.Title)
            .MinimumLength(3)
            .WithMessage("Title must be minimum 3 characters long");

        RuleFor(x => x.Description)
            .MinimumLength(3)
            .WithMessage("Description must be minimum 3 characters long");

        RuleFor(x => x.Tags)
            .NotNull()
            .WithMessage("Tags must not be null");

        RuleFor(x => x.LowerYearRange)
            .GreaterThan(0)
            .WithMessage("LowerYearRange must be greater than 0");

        RuleFor(x => x.UpperYearRange)
            .GreaterThan(c => c.LowerYearRange + 50)
            .WithMessage("UpperYearRange must be at least 50 years later than the lower range");
    }
}