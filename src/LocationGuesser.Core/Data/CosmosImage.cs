using LocationGuesser.Core.Domain;
using Newtonsoft.Json;

namespace LocationGuesser.Core.Data;

public class CosmosImage
{
    [JsonProperty("pk")] public string Pk => SetId.ToString();
    [JsonProperty("id")] public required int Number { get; set; }
    [JsonProperty("year")] public required int Year { get; set; }
    [JsonProperty("latitude")] public required double Latitude { get; set; }
    [JsonProperty("longitude")] public required double Longitude { get; set; }
    [JsonProperty("license")] public required string License { get; set; }
    [JsonProperty("description")] public required string Description { get; set; }
    [JsonProperty("setId")] public Guid SetId { get; set; }

    private CosmosImage()
    {

    }

    public static CosmosImage FromImage(Image image)
    {
        return new()
        {
            SetId = image.SetId,
            Number = image.Number,
            Latitude = image.Latitude,
            Longitude = image.Longitude,
            Year = image.Year,
            License = image.License,
            Description = image.Description
        };
    }

    public Image ToImage()
    {
        return new(SetId, Number, Year, Latitude, Longitude, Description, License);
    }
}