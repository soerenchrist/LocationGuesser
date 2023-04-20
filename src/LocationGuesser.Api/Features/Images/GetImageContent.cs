using FluentResults;
using FluentValidation;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Data.Abstractions;
using MediatR;

namespace LocationGuesser.Api.Features.Images;

public record GetImageContentQuery(Guid SetId, int Number) : IRequest<Result<Stream>>;

public class GetImageContentQueryHandler : IRequestHandler<GetImageContentQuery, Result<Stream>>
{
    private readonly IBlobRepository _blobRepository;
    private readonly IValidator<GetImageContentQuery> _validator;
    private readonly ILogger<GetImageContentQueryHandler> _logger;

    public GetImageContentQueryHandler(IBlobRepository blobRepository,
        IValidator<GetImageContentQuery> validator,
        ILogger<GetImageContentQueryHandler> logger)
    {
        _blobRepository = blobRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Stream>> Handle(GetImageContentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Downloading image {Number} from set {SetId}", request.Number, request.SetId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogDebug("Invalid request: {ValidationErrors}", validationResult.Errors);
            return validationResult.ToResult<Stream>();
        }

        var result =
            await _blobRepository.DownloadImageAsync($"{request.SetId}/{request.Number}.jpg", cancellationToken);
        return result;
    }
}

public class GetImageContentQueryValidator : AbstractValidator<GetImageContentQuery>
{
    public GetImageContentQueryValidator()
    {
        RuleFor(x => x.Number).GreaterThan(0);
    }
}