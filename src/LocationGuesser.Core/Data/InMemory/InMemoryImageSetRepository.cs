using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Core.Data.InMemory;

public class InMemoryImageSetRepository : IImageSetRepository
{
    private readonly Dictionary<string, ImageSet> _images = new();

    public Task<Result<ImageSet>> GetImageSetAsync(string slug, CancellationToken cancellationToken)
    {
        if (_images.TryGetValue(slug, out var image)) return Task.FromResult(Result.Ok(image));

        var result = Result.Fail<ImageSet>(new NotFoundError("ImageSet not found"));
        return Task.FromResult(result);
    }

    public Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        var list = _images.Values.ToList();
        return Task.FromResult(Result.Ok(list));
    }

    public Task<Result<ImageSet>> AddImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        if (_images.ContainsKey(imageSet.Slug))
            return Task.FromResult(Result.Fail<ImageSet>("ImageSet already exists"));

        _images.Add(imageSet.Slug, imageSet);
        return Task.FromResult(Result.Ok(imageSet));
    }

    public Task<Result> UpdateImageSetAsync(ImageSet imageSet, CancellationToken cancellationToken)
    {
        if (!_images.ContainsKey(imageSet.Slug))
            return Task.FromResult(Result.Fail("ImageSet does not exist"));
        _images[imageSet.Slug] = imageSet;
        return Task.FromResult(Result.Ok());
    }

    public Task<Result> DeleteImageSetAsync(string slug, CancellationToken cancellationToken)
    {
        if (!_images.ContainsKey(slug))
            return Task.FromResult(Result.Fail("ImageSet does not exist"));

        _images.Remove(slug);

        return Task.FromResult(Result.Ok());
    }
}