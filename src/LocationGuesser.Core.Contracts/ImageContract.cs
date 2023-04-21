namespace LocationGuesser.Core.Contracts;

public class ImageContract
{
    public required Guid SetId { get; set; }
    public required int Number { get; set; }
    public required string Description { get; set; }
    public required string License { get; set; }
    public required int Year { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public required string Url { get; set; }

}