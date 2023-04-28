using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using LocationGuesser.Core.Services;
using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Tests.Services;

public class DailyChallengeServiceTests
{
    private readonly DailyChallengeService _cut;
    private readonly IDailyChallengeRepository _challengeRepository = Substitute.For<IDailyChallengeRepository>();
    private readonly IImageSetRepository _imageSetRepository = Substitute.For<IImageSetRepository>();
    private readonly IRandom _random = Substitute.For<IRandom>();

    private readonly DateTime _date = new DateTime(2023, 3, 1);

    public DailyChallengeServiceTests()
    {
        _random.Next(1, 5, Arg.Is<HashSet<int>>(x => !x.Any())).Returns(1);
        _random.Next(1, 10, Arg.Any<HashSet<int>>())
            .Returns(1, 2, 3, 4, 5);
        _imageSetRepository.ListImageSetsAsync(default)
            .Returns(CreateImageSets());
        _cut = new DailyChallengeService(_challengeRepository, _imageSetRepository, _random);
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnDailyChallenge_WhenAlreadyOneExists()
    {
        var challenge = CreateDailyChallenge();
        _challengeRepository.GetDailyChallengeAsync(_date, default)
            .Returns(challenge);

        var result = await _cut.GetDailyChallengeAsync(_date, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(challenge);
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnError_WhenRepositoryReturnsUnknownError()
    {
        _challengeRepository.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Fail<DailyChallenge>("Something went wrong"));

        var result = await _cut.GetDailyChallengeAsync(_date, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldCreateANewChallenge_WhenRepositoryReturnsNotFound()
    {
        _challengeRepository.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Fail<DailyChallenge>(new NotFoundError("Not found")));
        _challengeRepository.AddDailyChallengeAsync(Arg.Any<DailyChallenge>(), default)
            .Returns(CreateDailyChallenge());

        await _cut.GetDailyChallengeAsync(_date, default);

        await _challengeRepository.Received(1).AddDailyChallengeAsync(Arg.Any<DailyChallenge>(), default);
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldPickARandomImageSet_WhenRepositoryReturnsNotFound()
    {
        _challengeRepository.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Fail<DailyChallenge>(new NotFoundError("Not found")));
        _challengeRepository.AddDailyChallengeAsync(Arg.Any<DailyChallenge>(), default)
            .Returns(CreateDailyChallenge());

        await _cut.GetDailyChallengeAsync(_date, default);

        await _challengeRepository.Received(1)
            .AddDailyChallengeAsync(Arg.Is<DailyChallenge>(x =>
                x.Slug == "slug-2"
            ), default);
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldPickFiveRandomImages_WhenRepositoryReturnsNotFound()
    {
        _challengeRepository.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Fail<DailyChallenge>(new NotFoundError("Not found")));
        _challengeRepository.AddDailyChallengeAsync(Arg.Any<DailyChallenge>(), default)
            .Returns(CreateDailyChallenge());

        await _cut.GetDailyChallengeAsync(_date, default);

        await _challengeRepository.Received(1)
            .AddDailyChallengeAsync(Arg.Is<DailyChallenge>(x =>
                x.ImageNumbers.Count == 5
            ), default);
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnFailed_WhenImageSetListCannotBeRetrieved()
    {
        _challengeRepository.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Fail<DailyChallenge>(new NotFoundError("Not found")));
        _imageSetRepository.ListImageSetsAsync(default)
            .Returns(Result.Fail<List<ImageSet>>("Something failed"));
        _challengeRepository.AddDailyChallengeAsync(Arg.Any<DailyChallenge>(), default)
            .Returns(CreateDailyChallenge());

        var result = await _cut.GetDailyChallengeAsync(_date, default);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetDailyChallengeAsync_ShouldReturnCreatedChallenge_WhenNoneExistsYet()
    {
        _challengeRepository.GetDailyChallengeAsync(_date, default)
            .Returns(Result.Fail<DailyChallenge>(new NotFoundError("Not found")));
        _challengeRepository.AddDailyChallengeAsync(Arg.Any<DailyChallenge>(), default)
            .Returns(CreateDailyChallenge());

        var result = await _cut.GetDailyChallengeAsync(_date, default);

        result.Value.Slug.Should().Be("slug-2");
        result.Value.ImageNumbers.Should().BeEquivalentTo(new List<int> { 1, 2, 3, 4, 5 });
    }

    private DailyChallenge CreateDailyChallenge()
    {
        return new DailyChallenge("slug", new List<int> { 1, 2, 3 });
    }

    private List<ImageSet> CreateImageSets()
    {
        return Enumerable.Range(1, 5)
            .Select(x => new ImageSet($"slug-{x}", $"Title {x}", $"Description {x}", $"Tags {x}", 1900, 2000, 10))
            .ToList();
    }
}