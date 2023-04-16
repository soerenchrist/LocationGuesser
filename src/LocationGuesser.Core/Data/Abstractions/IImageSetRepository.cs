using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Data.Abstractions;

public interface IImageSetRepository
{
    Task<Result<ImageSet?>> GetImageSetAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ImageSet>> ListImageSetsAsync(CancellationToken cancellationToken);
    Task<Result> AddImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken);
}