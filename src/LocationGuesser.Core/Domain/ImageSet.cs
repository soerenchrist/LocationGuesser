using System.Text.Json.Serialization;

namespace LocationGuesser.Core.Domain;

public record ImageSet(Guid Id, string Title, string Description, string Tags);

public class CosmosImageSet
{
    public Guid id => Id;
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Tags { get; set; }
}