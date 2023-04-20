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
    private readonly IRandom _random;

    public ImageService(IBlobRepository blobRepository,
        IImageRepository imageRepository,
        IImageSetRepository imageSetRepository,
        IRandom random)
    {
        _blobRepository = blobRepository;
        _imageRepository = imageRepository;
        _imageSetRepository = imageSetRepository;
        _random = random;
    }

    public async Task<Result> AddImageToImageSetAsync(ImageSet imageSet, Image image, Stream fileContent,
        CancellationToken cancellationToken)
    {
        var filename = $"{imageSet.Id}_{image.Number}.png";
        var uploadResult = await _blobRepository.UploadImageAsync(filename, fileContent, cancellationToken);
        if (uploadResult.IsFailed) return Result.Merge(uploadResult, Result.Fail("Failed to upload image to storage"));

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

    public async Task<Result<List<Image>>> GetGameSetAsync(Guid imageSetId, int imageCount, CancellationToken cancellationToken)
    {
        var imageSetResult = await _imageSetRepository.GetImageSetAsync(imageSetId, cancellationToken);
        if (imageSetResult.IsFailed)
        {
            return Result.Merge(imageSetResult, Result.Fail<List<Image>>("Failed to get image set"));
        }

        var imageSet = imageSetResult.Value;
        if (imageSet.ImageCount < imageCount)
        {
            return Result.Fail<List<Image>>("Image set does not contain enough images");
        }

        var usedNumbers = new HashSet<int>();
        while (usedNumbers.Count < imageCount)
        {
            var number = _random.Next(1, imageSet.ImageCount + 1, usedNumbers);
            usedNumbers.Add(number);
        }

        var tasks = usedNumbers.Select(number => _imageRepository.GetImageAsync(imageSetId, number, cancellationToken));
        await Task.WhenAll(tasks);

        if (tasks.Any(task => task.Result.IsFailed))
        {
            return Result.Fail<List<Image>>("Failed to get images");
        }

        return Result.Ok(tasks.Select(task => task.Result.Value).ToList());
    }
}