using LocationGuesser.Core.Domain;
using Newtonsoft.Json;

namespace LocationGuesser.Core.Data;

public class CosmosImageSet
{
    [JsonProperty("pk")] public string Pk => "IMAGESETS";
    [JsonProperty("id")] public required Guid Id { get; init; }
    [JsonProperty("title")] public required string Title { get; init; }
    [JsonProperty("description")] public required string Description { get; init; }
    [JsonProperty("tags")] public required string Tags { get; init; }
    [JsonProperty("lowerYearRange")] public required int LowerYearRange { get; init; }
    [JsonProperty("upperYearRange")] public required int UpperYearRange { get; init; }
    [JsonProperty("imageCount")] public required int ImageCount { get; init; }
    [JsonProperty("type")] public string Type => "ImageSet";

    private CosmosImageSet()
    {
    }

    public static CosmosImageSet FromImageSet(ImageSet imageSet)
    {
        return new()
        {
            Id = imageSet.Id,
            Title = imageSet.Title,
            Description = imageSet.Description,
            Tags = imageSet.Tags,
            LowerYearRange = imageSet.LowerYearRange,
            UpperYearRange = imageSet.UpperYearRange,
            ImageCount = imageSet.ImageCount,
        };
    }

    public ImageSet ToImageSet()
    {
        return new(Id, Title, Description, Tags, LowerYearRange, UpperYearRange, ImageCount);
    }
}