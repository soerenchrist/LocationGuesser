using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Core.Contracts;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Api.Mappings;

public static class ImageSetMappings
{
    public static ImageSetContract ToContract(this ImageSet imageSet)
    {
        return new ImageSetContract
        {
            Id = imageSet.Id,
            Title = imageSet.Title,
            Description = imageSet.Description,
            Tags = imageSet.Tags,
            LowerYearRange = imageSet.LowerYearRange,
            UpperYearRange = imageSet.UpperYearRange,
            ImageCount = imageSet.ImageCount
        };
    }

    public static ImageSet ToDomain(this CreateImageSetCommand command)
    {
        return new ImageSet(Guid.NewGuid(), command.Title, command.Description, command.Tags, command.LowerYearRange, command.UpperYearRange, 0);
    }
}