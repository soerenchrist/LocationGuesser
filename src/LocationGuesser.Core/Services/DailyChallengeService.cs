using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Extensions;
using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Core.Services;

public class DailyChallengeService : IDailyChallengeService
{
    private readonly IDailyChallengeRepository _challengeRepository;
    private readonly IImageSetRepository _imageSetRepository;
    private readonly IRandom _random;

    private const int NumberOfImages = 5;

    public DailyChallengeService(IDailyChallengeRepository challengeRepository,
        IImageSetRepository imageSetRepository,
        IRandom random)
    {
        _challengeRepository = challengeRepository;
        _imageSetRepository = imageSetRepository;
        _random = random;
    }

    public async Task<Result<DailyChallenge>> GetDailyChallengeAsync(DateTime dateTime,
        CancellationToken cancellationToken)
    {
        var result = await _challengeRepository.GetDailyChallengeAsync(dateTime, cancellationToken);
        if (result.IsNotFound())
        {
            var imageSet = await PickRandomImageSet(cancellationToken);
            if (imageSet.IsFailed) return Result.Fail<DailyChallenge>(imageSet.Errors);

            var images = PickRandomImageIndices(imageSet.Value);
            var dailyChallenge = new DailyChallenge(imageSet.Value.Slug, images);
            await _challengeRepository.AddDailyChallengeAsync(dailyChallenge, cancellationToken);
            return dailyChallenge;
        }

        return result;
    }

    private async Task<Result<ImageSet>> PickRandomImageSet(CancellationToken cancellationToken)
    {
        var imageSets = await _imageSetRepository.ListImageSetsAsync(cancellationToken);
        if (imageSets.IsFailed) return Result.Fail<ImageSet>(imageSets.Errors);

        var randomIndex = _random.Next(0, imageSets.Value.Count, new HashSet<int>());
        return imageSets.Value[randomIndex];
    }

    private List<int> PickRandomImageIndices(ImageSet imageSet)
    {
        var imageIndices = new HashSet<int>();
        while (imageIndices.Count < NumberOfImages)
        {
            imageIndices.Add(_random.Next(0, imageSet.ImageCount, imageIndices));
        }

        return imageIndices.ToList();
    }
}