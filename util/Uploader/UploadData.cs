using LocationGuesser.Core.Domain;

namespace Uploader;

public class UploadData
{
    public List<ImageSet> ImageSets { get; } = new();
    public List<Image> Images { get; } = new();
    public List<ImageContent> Files { get; } = new();
}

public record ImageSetInfo(string Title, string Description, string Tags, int lowerYearRange, int upperYearRange);
public record ImageInfo(string Description, int Year, double Latitude, double Longitude, string License);
public record ImageContent(string Filename, Guid SetId, int Number);