using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using MediatR;

namespace LocationGuesser.Api.Features.ImageSets;

public record GetImageSetQuery(Guid Id) : IRequest<Result<ImageSet>>;

public class GetImageSetQueryHandler : IRequestHandler<GetImageSetQuery, Result<ImageSet>>
{
    private readonly IImageSetRepository _imageSetRepository;

    public GetImageSetQueryHandler(IImageSetRepository imageSetRepository)
    {
        _imageSetRepository = imageSetRepository;
    }

    public async Task<Result<ImageSet>> Handle(GetImageSetQuery request, CancellationToken cancellationToken)
    {
        var result = await _imageSetRepository.GetImageSetAsync(request.Id, cancellationToken);
        return result;
    }
}