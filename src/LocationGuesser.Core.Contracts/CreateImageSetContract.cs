namespace LocationGuesser.Core.Contracts;

public class CreateImageSetContract
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Tags { get; init; }
    public int LowerYearRange { get; init; }
    public int UpperYearRange { get; init; }
}