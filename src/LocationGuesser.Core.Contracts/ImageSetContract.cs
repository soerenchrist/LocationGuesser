namespace LocationGuesser.Api.Contracts;

public class ImageSetContract
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Tags { get; init; }
    public required int LowerYearRange { get; init; }
    public required int UpperYearRange { get; init; }
}