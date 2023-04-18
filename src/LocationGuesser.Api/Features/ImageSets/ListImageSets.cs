using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using MediatR;

namespace LocationGuesser.Api.Features.ImageSets;

public record ListImageSetsQuery : IRequest<Result<List<ImageSet>>>;

public class ListImageSetsQueryHandler : IRequestHandler<ListImageSetsQuery, Result<List<ImageSet>>>
{
    private readonly IImageSetRepository _imageSetRepository;
    public ListImageSetsQueryHandler(IImageSetRepository imageSetRepository)
    {
        _imageSetRepository = imageSetRepository;
    }

    public async Task<Result<List<ImageSet>>> Handle(ListImageSetsQuery request, CancellationToken cancellationToken)
    {
        return await _imageSetRepository.ListImageSetsAsync(cancellationToken);
    }
}