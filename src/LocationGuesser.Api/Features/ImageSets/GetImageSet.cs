using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using MediatR;

namespace LocationGuesser.Api.Features.ImageSets;

public record GetImageSetQuery(string SetSlug) : IRequest<Result<ImageSet>>;

public class GetImageSetQueryHandler : IRequestHandler<GetImageSetQuery, Result<ImageSet>>
{
    private IImageSetRepository _imageSetRepository;
    private ILogger<GetImageSetQueryHandler> _logger;

    public GetImageSetQueryHandler(IImageSetRepository imageSetRepository,
        ILogger<GetImageSetQueryHandler> logger)
    {
        _imageSetRepository = imageSetRepository;
        _logger = logger;
    }

    public Task<Result<ImageSet>> Handle(GetImageSetQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting image set {SetSlug}", request.SetSlug);
        return _imageSetRepository.GetImageSetAsync(request.SetSlug, cancellationToken);
    }
}