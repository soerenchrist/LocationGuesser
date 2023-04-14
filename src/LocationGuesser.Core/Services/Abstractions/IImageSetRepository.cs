using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Services.Abstractions;

public interface IImageSetRepository
{
    Task<ImageSet?> GetImageSetAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ImageSet>> ListImageSetsAsync(CancellationToken cancellationToken);
}