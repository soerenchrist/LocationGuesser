using LocationGuesser.Core.Domain;

namespace LocationGuesser.Api.Mappings;

public static class ImageSetMappings
{
    public static GetImageSetResponse ToResponse(this ImageSet imageSet)
    {
        return new GetImageSetResponse
        {
            Id = imageSet.Id.ToString(),
            Title = imageSet.Title,
            Description = imageSet.Description,
            Tags = imageSet.Tags
        };
    }
    
}