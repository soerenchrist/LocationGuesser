using LocationGuesser.Core.Domain;
using Newtonsoft.Json;

namespace LocationGuesser.Core.Data;

public class CosmosImageSet
{
    [JsonProperty("pk")] public string Pk => Id.ToString();
    [JsonProperty("id")] public required Guid Id { get; init; }
    [JsonProperty("title")] public required string Title { get; init; }
    [JsonProperty("description")] public required string Description { get; init; }
    [JsonProperty("tags")] public required string Tags { get; init; }

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
            Tags = imageSet.Tags
        };
    }

    public ImageSet ToImageSet()
    {
        return new(Id, Title, Description, Tags);
    }
}