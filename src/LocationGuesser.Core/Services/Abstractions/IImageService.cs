using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Services.Abstractions;

public interface IImageService
{
    Task<Result> AddImageToImageSetAsync(Guid setId, Image image, Stream fileContent,
        CancellationToken cancellationToken);
}