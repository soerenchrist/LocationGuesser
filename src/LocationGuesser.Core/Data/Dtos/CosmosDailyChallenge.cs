using Newtonsoft.Json;

namespace LocationGuesser.Core.Data.Dtos;

public class CosmosDailyChallenge
{
    [JsonProperty("pk")] public string Pk => "DAILYCHALLENGES";
    [JsonProperty("id")] public string Id => Date.ToString("O");
    [JsonProperty("date")] public required DateTime Date { get; set; }
    [JsonProperty("setSlug")] public required string SetSlug { get; set; }
    [JsonProperty("images")] public required string ImagesCsv { get; set; }
    [JsonProperty("type")] public string Type => "DailyChallenge";
    [JsonProperty("ttl")] public int TimeToLive => 24 * 60 * 60;
}