using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Core.Services;

public class ImageService : IImageService
{
    private readonly IBlobRepository _blobRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IImageSetRepository _imageSetRepository;

    public ImageService(IBlobRepository blobRepository,
        IImageRepository imageRepository,
        IImageSetRepository imageSetRepository)
    {
        _blobRepository = blobRepository;
        _imageRepository = imageRepository;
        _imageSetRepository = imageSetRepository;
    }

    public async Task<Result> AddImageToImageSetAsync(ImageSet imageSet, Image image, Stream fileContent, CancellationToken cancellationToken)
    {
        var filename = $"{imageSet.Id}_{image.Number}.png";
        var uploadResult = await _blobRepository.UploadImageAsync(filename, fileContent, cancellationToken);
        if (uploadResult.IsFailed)
        {
            return Result.Merge(uploadResult, Result.Fail("Failed to upload image to storage"));
        }

        var imageAddResult = await _imageRepository.AddImageAsync(image, cancellationToken);
        if (imageAddResult.IsFailed)
        {
            _ = await _blobRepository.DeleteImageAsync(filename, cancellationToken);
            return Result.Merge(imageAddResult, Result.Fail("Failed to add image to image set"));
        }

        var updatedImageSet = imageSet with { ImageCount = imageSet.ImageCount + 1 };
        var imageSetUpdateResult = await _imageSetRepository.UpdateImageSetAsync(updatedImageSet, cancellationToken);
        if (imageSetUpdateResult.IsFailed)
        {
            _ = await _blobRepository.DeleteImageAsync(filename, cancellationToken);
            _ = await _imageRepository.DeleteImageAsync(image, cancellationToken);
            return Result.Merge(imageSetUpdateResult, Result.Fail("Failed to update image sets image count"));
        }

        return Result.Ok();
    }
}