using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Services.Abstractions;

public interface IImageService
{
    Task<Result> AddImageToImageSetAsync(ImageSet imageSet, Image image, Stream fileContent,
        CancellationToken cancellationToken);
}