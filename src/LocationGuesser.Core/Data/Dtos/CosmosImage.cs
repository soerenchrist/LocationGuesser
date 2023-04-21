using LocationGuesser.Core.Domain;
using Newtonsoft.Json;

namespace LocationGuesser.Core.Data.Dtos;

public class CosmosImage
{
    private CosmosImage()
    {
    }

    [JsonProperty("pk")] public string Pk => SetId.ToString();
    [JsonProperty("id")] public string InternalId => Number.ToString();
    [JsonProperty("number")] public required int Number { get; set; }
    [JsonProperty("year")] public required int Year { get; set; }
    [JsonProperty("latitude")] public required double Latitude { get; set; }
    [JsonProperty("longitude")] public required double Longitude { get; set; }
    [JsonProperty("license")] public required string License { get; set; }
    [JsonProperty("description")] public required string Description { get; set; }
    [JsonProperty("setId")] public required Guid SetId { get; set; }
    [JsonProperty("url")] public required string Url { get; set; }
    [JsonProperty("type")] public string Type => "Image";

    public static CosmosImage FromImage(Image image)
    {
        return new CosmosImage
        {
            SetId = image.SetId,
            Number = image.Number,
            Latitude = image.Latitude,
            Longitude = image.Longitude,
            Year = image.Year,
            License = image.License,
            Description = image.Description,
            Url = image.Url
        };
    }

    public Image ToImage()
    {
        return new Image(SetId, Number, Year, Latitude, Longitude, Description, License, Url);
    }
}