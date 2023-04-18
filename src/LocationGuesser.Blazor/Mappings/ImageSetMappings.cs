using LocationGuesser.Core.Contracts;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Blazor.Mappings;

public static class ImageSetMappings
{
    public static ImageSet ToDomain(this ImageSetContract contract)
    {
        return new ImageSet(contract.Id, contract.Title, contract.Description, contract.Tags, contract.LowerYearRange, contract.UpperYearRange, contract.ImageCount);
    }
}