using FluentResults;
using LocationGuesser.Api.Features.Game;
using LocationGuesser.Core.Contracts;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Tests.Features.Game;

public class GetDailyChallengeQueryHandlerTests
{
    private readonly GetDailyChallengeQueryHandler _cut;
    private readonly IDailyChallengeService _challengeService = Substitute.For<IDailyChallengeService>();
    private readonly IImageSetRepository _imageSetRepository = Substitute.For<IImageSetRepository>();
    private readonly IImageRepository _imageRepository = Substitute.For<IImageRepository>();
    private readonly DateTime _date = new DateTime(2023, 3, 1);

    public GetDailyChallengeQueryHandlerTests()
    {
        _cut = new GetDailyChallengeQueryHandler(_challengeService, _imageSetRepository, _imageRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenServiceFails()
    {
        _challengeService.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Fail<DailyChallenge>("error"));

        var result = await _cut.Handle(new GetDailyChallengeQuery(_date), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldGetImageSetFromRepo_WhenServiceReturnsChallenge()
    {
        var challenge = CreateDailyChallenge();
        _challengeService.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Ok(challenge));
        _imageSetRepository.GetImageSetAsync("slug", default)
            .Returns(Result.Ok(CreateImageSet()));
        _imageRepository.GetImageAsync("slug", Arg.Any<int>(), default)
            .Returns(Result.Ok(CreateImage(1)));

        await _cut.Handle(new GetDailyChallengeQuery(_date), default);

        await _imageSetRepository.Received(1).GetImageSetAsync("slug", default);
    }

    [Fact]
    public async Task Handle_ShouldGetImagesFromRepo_WhenServiceReturnsChallenge()
    {
        var challenge = CreateDailyChallenge();
        _challengeService.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Ok(challenge));
        _imageSetRepository.GetImageSetAsync("slug", default)
            .Returns(Result.Ok<ImageSet>(CreateImageSet()));
        _imageRepository.GetImageAsync("slug", Arg.Any<int>(), default)
            .Returns(Result.Ok(CreateImage(1)));

        await _cut.Handle(new GetDailyChallengeQuery(_date), default);

        foreach (var number in challenge.ImageNumbers)
        {
            await _imageRepository.Received(1).GetImageAsync("slug", number, default);
        }
    }


    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenImageSetCannotBeFetched()
    {
        var challenge = CreateDailyChallenge();
        _challengeService.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Ok(challenge));
        _imageSetRepository.GetImageSetAsync("slug", default)
            .Returns(Result.Fail<ImageSet>("Failed"));

        var result = await _cut.Handle(new GetDailyChallengeQuery(_date), default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailed_WhenAnyImageCannotBeFetched()
    {
        var challenge = CreateDailyChallenge();
        _challengeService.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Ok(challenge));
        _imageSetRepository.GetImageSetAsync("slug", default)
            .Returns(Result.Ok(CreateImageSet()));
        _imageRepository.GetImageAsync("slug", 1, default)
            .Returns(Result.Ok(CreateImage(1)));
        _imageRepository.GetImageAsync("slug", Arg.Is<int>(x => x > 1), default)
            .Returns(Result.Fail<Image>("Failed"));

        var result = await _cut.Handle(new GetDailyChallengeQuery(_date), default);

        result.IsFailed.Should().BeTrue();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnChallenge_WhenAllCallsSucceed()
    {
        var challenge = CreateDailyChallenge();
        var imageSet = CreateImageSet();
        _challengeService.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Ok(challenge));
        _imageSetRepository.GetImageSetAsync("slug", default)
            .Returns(Result.Ok(imageSet));
        _imageRepository.GetImageAsync("slug", Arg.Any<int>(), default)
            .Returns(Result.Ok(CreateImage(1)));

        var result = await _cut.Handle(new GetDailyChallengeQuery(_date), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.ImageSet.Should().Be(imageSet);
        result.Value.Images.Should().HaveCount(5);
    }

    private Image CreateImage(int number)
    {
        return new Image("slug", number, 2000, 49, 6, "description", "license", "url");
    }

    private DailyChallenge CreateDailyChallenge()
    {
        return new DailyChallenge("slug", new List<int> { 1, 2, 3, 4, 5 });
    }

    private ImageSet CreateImageSet()
    {
        return new ImageSet("slug", "Title", "Description", "Tags", 1900, 2000, 10);
    }
}