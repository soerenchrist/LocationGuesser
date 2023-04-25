namespace LocationGuesser.Core.Domain;

public record ImageSet(string Slug, string Title, string Description, string Tags, int LowerYearRange, int UpperYearRange,
    int ImageCount);