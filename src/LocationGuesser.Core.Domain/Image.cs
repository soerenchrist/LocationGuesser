namespace LocationGuesser.Core.Domain;

public record Image(string SetSlug, int Number, int Year, double Latitude, double Longitude, string Description,
    string License, string Url);