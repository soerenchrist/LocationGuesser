using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Data.Abstractions;

public interface IImageRepository
{
    Task<Result<List<Image>>> ListImagesAsync(string setSlug, CancellationToken cancellationToken);
    Task<Result<Image>> GetImageAsync(string setSlug, int number, CancellationToken cancellationToken);
    Task<Result> AddImageAsync(Image image, CancellationToken cancellationToken);
    Task<Result> DeleteImageAsync(Image image, CancellationToken cancellationToken);
}