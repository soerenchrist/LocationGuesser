using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using MediatR;

namespace LocationGuesser.Api.Features.ImageSets;

public record DeleteImageSetCommand(Guid Id) : IRequest<Result>;

public class DeleteImageSetCommandHandler : IRequestHandler<DeleteImageSetCommand, Result>
{
    private readonly IImageSetRepository _imageSetRepository;

    public DeleteImageSetCommandHandler(IImageSetRepository imageSetRepository)
    {
        _imageSetRepository = imageSetRepository;
    }

    public async Task<Result> Handle(DeleteImageSetCommand request, CancellationToken cancellationToken)
    {
        var result = await _imageSetRepository.DeleteImageSetAsync(request.Id, cancellationToken);
        return result;
    }
}