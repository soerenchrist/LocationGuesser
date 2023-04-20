using LocationGuesser.Api.Features.Images;
using LocationGuesser.Core.Contracts;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Api.Mappings;

public static class ImageMappings
{
    public static Image ToDomain(this CreateImageCommand command)
    {
        return new Image(
            command.SetId,
            1,
            command.Year,
            command.Latitude,
            command.Longitude,
            command.Description!,
            command.License!);
    }

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