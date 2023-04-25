using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Data.Abstractions;

public interface IImageSetRepository
{
    Task<Result<ImageSet>> GetImageSetAsync(string slug, CancellationToken cancellationToken);
    Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken);
    Task<Result<ImageSet>> AddImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken);
    Task<Result> UpdateImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken);
    Task<Result> DeleteImageSetAsync(string slug, CancellationToken cancellationToken);
}