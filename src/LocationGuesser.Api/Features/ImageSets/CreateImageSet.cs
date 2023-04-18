using FluentResults;
using FluentValidation;
using LocationGuesser.Api.Contracts;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using MediatR;

namespace LocationGuesser.Api.Features.ImageSets;

public class CreateImageSetCommandHandler : IRequestHandler<CreateImageSetContract, Result<ImageSet>>
{
    private readonly IValidator<CreateImageSetContract> _validator;
    private readonly IImageSetRepository _imageSetRepository;
    public CreateImageSetCommandHandler(IValidator<CreateImageSetContract> validator,
        IImageSetRepository imageSetRepository)
    {
        _validator = validator;
        _imageSetRepository = imageSetRepository;
    }

    public async Task<Result<ImageSet>> Handle(CreateImageSetContract message, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(message);
        if (!validationResult.IsValid)
        {
            return validationResult.ToResult<ImageSet>();
        }

        var result = await _imageSetRepository.AddImageSetAsync(message.ToImageSet(), default);
        return result;
    }
}