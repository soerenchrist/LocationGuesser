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
    private readonly IGameService _imageService;
    private readonly IValidator<GetGameQuery> _validator;
    private readonly ILogger<GetGameQueryHandler> _logger;

    public GetGameQueryHandler(IGameService imageService,
        IValidator<GetGameQuery> validator,
        ILogger<GetGameQueryHandler> logger)
    {
        _imageService = imageService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<Image>>> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting game set {SetId} with {ImageCount} images", request.SetId, request.ImageCount);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogDebug("Invalid request: {ValidationErrors}", validationResult.Errors);
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