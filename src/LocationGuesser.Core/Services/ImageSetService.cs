using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using Optional;

namespace LocationGuesser.Core.Services;

public class ImageSetService
{
    private readonly IImageSetRepository _repository;

    public ImageSetService(IImageSetRepository repository)
    {
        _repository = repository;
    }

    public Task<Result<ImageSet?>> GetImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        return _repository.GetImageSetAsync(id, cancellationToken);
    }

    public Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        return _repository.ListImageSetsAsync(cancellationToken);
    }
}