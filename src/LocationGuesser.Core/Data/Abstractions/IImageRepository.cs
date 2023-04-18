using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Data.Abstractions;

public interface IImageRepository
{
    Task<Result<List<Image>>> ListImagesAsync(Guid setId, CancellationToken cancellationToken);
    Task<Result<Image>> GetImageAsync(Guid setId, int number, CancellationToken cancellationToken);
    Task<Result> AddImageAsync(Image image, CancellationToken cancellationToken);
    Task<Result> DeleteImageAsync(Image image, CancellationToken cancellationToken);
}