using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Core.Services;

public class GameService : IGameService
{
    private readonly IBlobRepository _blobRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IImageSetRepository _imageSetRepository;
    private readonly IRandom _random;

    public GameService(IBlobRepository blobRepository,
        IImageRepository imageRepository,
        IImageSetRepository imageSetRepository,
        IRandom random)
    {
        _blobRepository = blobRepository;
        _imageRepository = imageRepository;
        _imageSetRepository = imageSetRepository;
        _random = random;
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