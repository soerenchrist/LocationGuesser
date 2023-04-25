using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace LocationGuesser.Core.Services;

public class GameService : IGameService
{
    private readonly IImageRepository _imageRepository;
    private readonly IImageSetRepository _imageSetRepository;
    private readonly IRandom _random;
    private readonly ILogger<GameService> _logger;

    public GameService(
        IImageRepository imageRepository,
        IImageSetRepository imageSetRepository,
        IRandom random,
        ILogger<GameService> logger)
    {
        _imageRepository = imageRepository;
        _imageSetRepository = imageSetRepository;
        _random = random;
        _logger = logger;
    }

    public async Task<Result<List<Image>>> GetGameSetAsync(string imageSetSlug, int imageCount,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting game set {ImageSetId} with {ImageCount} images", imageSetSlug, imageCount);
        var imageSetResult = await _imageSetRepository.GetImageSetAsync(imageSetSlug, cancellationToken);
        if (imageSetResult.IsFailed)
        {
            _logger.LogError("Failed to get image set");
            return Result.Merge(imageSetResult, Result.Fail<List<Image>>("Failed to get image set"));
        }

        var imageSet = imageSetResult.Value;
        if (imageSet.ImageCount < imageCount)
        {
            _logger.LogError("Image set does not contain enough images");
            return Result.Fail<List<Image>>("Image set does not contain enough images");
        }

        var usedNumbers = new HashSet<int>();
        while (usedNumbers.Count < imageCount)
        {
            var number = _random.Next(1, imageSet.ImageCount, usedNumbers);
            usedNumbers.Add(number);
        }

        var tasks = usedNumbers.Select(number => _imageRepository.GetImageAsync(imageSetSlug, number, cancellationToken))
            .ToList();
        await Task.WhenAll(tasks);

        if (tasks.Any(task => task.Result.IsFailed))
        {
            var failedTasks = tasks.Where(task => task.Result.IsFailed).ToList();
            var errors = new List<IError>();
            foreach (var failedTask in failedTasks)
            {
                var result = await failedTask;
                _logger.LogError("Failed to get image: {Errors}", result.Errors);
                errors.AddRange(result.Errors);
            }

            return Result.Fail<List<Image>>(errors);
        }

        return Result.Ok(tasks.Select(task => task.Result.Value).ToList());
    }
}