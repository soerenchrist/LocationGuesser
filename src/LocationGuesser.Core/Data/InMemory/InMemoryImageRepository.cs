using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Core.Data.InMemory;

public class InMemoryImageRepository : IImageRepository
{
    private readonly List<Image> _images = new();

    public Task<Result<List<Image>>> ListImagesAsync(string setSlug, CancellationToken cancellationToken)
    {
        var list = new List<Image>(_images);
        return Task.FromResult(Result.Ok(list));
    }

    public Task<Result<Image>> GetImageAsync(string setSlug, int number, CancellationToken cancellationToken)
    {
        var image = _images.FirstOrDefault(x => x.SetSlug == setSlug && x.Number == number);
        var result = image == null
            ? Result.Fail<Image>(new NotFoundError("Image not found"))
            : Result.Ok(image);
        return Task.FromResult(result);
    }

    public Task<Result> AddImageAsync(Image image, CancellationToken cancellationToken)
    {
        if (_images.Any(x => x.SetSlug == image.SetSlug && x.Number == image.Number))
            return Task.FromResult(Result.Fail("Image already exists"));

        _images.Add(image);
        return Task.FromResult(Result.Ok());
    }

    public Task<Result> DeleteImageAsync(Image image, CancellationToken cancellationToken)
    {
        var existingImage = _images.FirstOrDefault(x => x.SetSlug == image.SetSlug && x.Number == image.Number);
        if (existingImage == null) return Task.FromResult(Result.Fail(new NotFoundError("Image not found")));

        return Task.FromResult(Result.Ok());
    }
}