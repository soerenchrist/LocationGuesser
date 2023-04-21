namespace LocationGuesser.Core.Domain;

public record Image(Guid SetId, int Number, int Year, double Latitude, double Longitude, string Description,
    string License, string Url);