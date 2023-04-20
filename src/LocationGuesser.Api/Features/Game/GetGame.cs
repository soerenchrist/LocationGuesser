using FluentResults;
using FluentValidation;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services.Abstractions;
using MediatR;

namespace LocationGuesser.Api.Features.Game;

public record GetGameQuery(Guid SetId, int ImageCount) : IRequest<Result<List<Image>>>;

public class GetGameQueryHandler : IRequestHandler<GetGameQuery, Result<List<Image>>>
{
    private readonly IImageService _imageService;
    private readonly IValidator<GetGameQuery> _validator;

    public GetGameQueryHandler(IImageService imageService,
        IValidator<GetGameQuery> validator)
    {
        _imageService = imageService;
        _validator = validator;
    }

    public async Task<Result<List<Image>>> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToResult<List<Image>>();
        }
        var result = await _imageService.GetGameSetAsync(request.SetId, request.ImageCount, cancellationToken);
        return result;
    }
}

public class GetGameQueryValidator : AbstractValidator<GetGameQuery>
{
    public GetGameQueryValidator()
    {
        RuleFor(x => x.SetId).NotEmpty();
        RuleFor(x => x.ImageCount).InclusiveBetween(3, 20);
    }
}