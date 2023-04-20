using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using MediatR;

namespace LocationGuesser.Api.Features.ImageSets;

public record ListImageSetsQuery : IRequest<Result<List<ImageSet>>>;

public class ListImageSetsQueryHandler : IRequestHandler<ListImageSetsQuery, Result<List<ImageSet>>>
{
    private readonly IImageSetRepository _imageSetRepository;
    private readonly ILogger<ListImageSetsQueryHandler> _logger;

    public ListImageSetsQueryHandler(IImageSetRepository imageSetRepository,
        ILogger<ListImageSetsQueryHandler> logger)
    {
        _imageSetRepository = imageSetRepository;
        _logger = logger;
    }

    public async Task<Result<List<ImageSet>>> Handle(ListImageSetsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Listing all image sets");
        return await _imageSetRepository.ListImageSetsAsync(cancellationToken);
    }
}