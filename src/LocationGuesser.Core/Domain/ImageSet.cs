namespace LocationGuesser.Core.Domain;

public record ImageSet(Guid Id, string Title, string Description, string Tags, int LowerYearRange, int UpperYearRange);