namespace LocationGuesser.Core.Services.Abstractions;
public interface IImageUrlService
{
    string GetImageUrl(string slug, int number);
}