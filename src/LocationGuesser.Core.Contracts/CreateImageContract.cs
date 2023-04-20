using Microsoft.AspNetCore.Http;

namespace LocationGuesser.Core.Contracts;

public class CreateImageContract
{
    public Guid SetId { get; set; }
    public int Number { get; set; }
    public string? Description { get; set; }
    public string? License { get; set; }
    public int Year { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public IFormFile? File { get; set; }
}