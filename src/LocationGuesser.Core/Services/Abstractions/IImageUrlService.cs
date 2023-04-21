namespace LocationGuesser.Core.Services.Abstractions;
public interface IImageUrlService
{
    string GetImageUrl(Guid setId, int number);
}