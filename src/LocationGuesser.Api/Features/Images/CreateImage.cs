using FluentResults;
using FluentValidation;
using LocationGuesser.Core.Domain;
using LocationGuesser.Api.Extensions;
using MediatR;
using LocationGuesser.Core.Services.Abstractions;
using LocationGuesser.Api.Mappings;

namespace LocationGuesser.Api.Features.Images;

public record CreateImageCommand(Guid SetId, string? Description, int Year, double Latitude, double Longitude, string? License) : IRequest<Result<Image>>;

public class CreateImageCommandHandler : IRequestHandler<CreateImageCommand, Result<Image>>
{
    private readonly IValidator<CreateImageCommand> _validator;
    private readonly IImageService _imageService;

    public CreateImageCommandHandler(
        IImageService imageService,
        IValidator<CreateImageCommand> validator)
    {
        _validator = validator;
        _imageService = imageService;
    }

    public async Task<Result<Image>> Handle(CreateImageCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToResult<Image>();
        }

        var image = request.ToDomain();
        var result = await _imageService.AddImageToImageSetAsync(request.SetId, image, Stream.Null, cancellationToken);
        if (result.IsFailed) return result;
        return Result.Ok(image);
    }
}

public class CreateImageCommandValidator : AbstractValidator<CreateImageCommand>
{
    public CreateImageCommandValidator()
    {
        RuleFor(x => x.SetId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Year).NotEmpty()
            .GreaterThan(0);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.License).NotNull();
    }
}