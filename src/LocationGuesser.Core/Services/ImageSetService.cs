using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services.Abstractions;
using Optional;

namespace LocationGuesser.Core.Services;

public class ImageSetService
{
    private readonly IImageSetRepository _repository;

    public ImageSetService(IImageSetRepository repository)
    {
        _repository = repository;
    }

    public async Task<Option<ImageSet>> GetImageSetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _repository.GetImageSetAsync(id, cancellationToken);
        return result?.Some() ?? Option.None<ImageSet>();
    }
    
    public async Task<List<ImageSet>> ListImageSetsAsync(CancellationToken cancellationToken)
    {
        return await _repository.ListImageSetsAsync(cancellationToken);
    }
}