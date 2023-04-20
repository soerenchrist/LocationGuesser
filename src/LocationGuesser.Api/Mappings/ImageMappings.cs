using LocationGuesser.Core.Contracts;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Api.Mappings;

public static class ImageMappings
{
    public static ImageContract ToContract(this Image image)
    {
        return new ImageContract
        {
            SetId = image.SetId,
            Number = image.Number,
            Description = image.Description,
            License = image.License,
            Year = image.Year,
            Latitude = image.Latitude,
            Longitude = image.Longitude
        };
    }
}