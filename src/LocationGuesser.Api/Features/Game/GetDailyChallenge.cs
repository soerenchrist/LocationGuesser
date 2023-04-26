using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services.Abstractions;
using MediatR;

namespace LocationGuesser.Api.Features.Game;

public record GetDailyChallengeQuery(DateTime Date) : IRequest<Result<DailyChallengeResult>>;

public class GetDailyChallengeQueryHandler : IRequestHandler<GetDailyChallengeQuery, Result<DailyChallengeResult>>
{
    private readonly IDailyChallengeService _dailyChallengeService;
    private readonly IImageSetRepository _imageSetRepository;
    private readonly IImageRepository _imageRepository;

    public GetDailyChallengeQueryHandler(IDailyChallengeService dailyChallengeService,
        IImageSetRepository imageSetRepository,
        IImageRepository imageRepository)
    {
        _dailyChallengeService = dailyChallengeService;
        _imageSetRepository = imageSetRepository;
        _imageRepository = imageRepository;
    }

    public async Task<Result<DailyChallengeResult>> Handle(GetDailyChallengeQuery request,
        CancellationToken cancellationToken)
    {
        var challengeResult = await _dailyChallengeService.GetDailyChallengeAsync(request.Date, cancellationToken);
        if (challengeResult.IsFailed)
        {
            return Result.Fail<DailyChallengeResult>(challengeResult.Errors);
        }

        var imageset = await _imageSetRepository.GetImageSetAsync(challengeResult.Value.Slug, cancellationToken);
        if (imageset.IsFailed)
        {
            return Result.Fail<DailyChallengeResult>(imageset.Errors);
        }

        var images = new List<Image>();
        foreach (var imageNumber in challengeResult.Value.ImageNumbers)
        {
            var image = await _imageRepository.GetImageAsync(challengeResult.Value.Slug, imageNumber,
                cancellationToken);
            if (image.IsFailed)
            {
                return Result.Fail<DailyChallengeResult>(image.Errors);
            }

            images.Add(image.Value);
        }

        return Result.Ok(new DailyChallengeResult(imageset.Value, images));
    }
}

public record DailyChallengeResult(ImageSet ImageSet, List<Image> Images);