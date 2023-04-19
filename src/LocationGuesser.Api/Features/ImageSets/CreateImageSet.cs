using FluentResults;
using FluentValidation;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Api.Mappings;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using MediatR;

namespace LocationGuesser.Api.Features.ImageSets;

public record CreateImageSetCommand(string Title, string Description, string Tags, int LowerYearRange,
    int UpperYearRange) : IRequest<Result<ImageSet>>;

public class CreateImageSetCommandHandler : IRequestHandler<CreateImageSetCommand, Result<ImageSet>>
{
    private readonly IImageSetRepository _imageSetRepository;
    private readonly IValidator<ImageSet> _validator;

    public CreateImageSetCommandHandler(IValidator<ImageSet> validator,
        IImageSetRepository imageSetRepository)
    {
        _validator = validator;
        _imageSetRepository = imageSetRepository;
    }

    public async Task<Result<ImageSet>> Handle(CreateImageSetCommand message, CancellationToken cancellationToken)
    {
        var imageSet = message.ToDomain();
        var validationResult = await _validator.ValidateAsync(imageSet, cancellationToken);
        if (!validationResult.IsValid) return validationResult.ToResult<ImageSet>();

        var result = await _imageSetRepository.AddImageSetAsync(imageSet, default);
        return result;
    }
}