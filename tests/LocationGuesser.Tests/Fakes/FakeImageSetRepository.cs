using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Tests.Fakes;

internal class FakeImageSetRepository : IImageSetRepository
{
    private readonly Dictionary<Guid, ImageSet> _imageSets = new();

    public Task<ImageSet?> GetImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        _imageSets.TryGetValue(id, out var imageSet);
        return Task.FromResult(imageSet);
    }

    public Task<List<ImageSet>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_imageSets.Values.ToList());
    }

    public void AddImageSet(ImageSet imageSet)
    {
        _imageSets.Add(imageSet.Id, imageSet);
    }
}