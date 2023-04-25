using System.Text.Json;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Services.Abstractions;

namespace Uploader;

public class DirectoryReader
{
    private readonly IImageUrlService _imageUrlService;
    public DirectoryReader(IImageUrlService imageUrlService)
    {
        _imageUrlService = imageUrlService;
    }

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public UploadData ReadDirectory(string path)
    {
        var uploadData = new UploadData();
        var directories = Directory.EnumerateDirectories(path);
        foreach (var directory in directories)
        {
            ReadImageSetDirectory(directory, uploadData);
        }

        return uploadData;
    }

    private void ReadImageSetDirectory(string path, UploadData uploadData)
    {
        var infoPath = Path.Combine(path, "info.json");
        if (!File.Exists(infoPath))
        {
            throw new Exception($"No info.json file found in {path}");
        }

        var info = JsonSerializer.Deserialize<ImageSetInfo>(File.ReadAllText(infoPath), _options);
        if (info == null) throw new Exception($"Could not deserialize info.json in {path}");

        int count = ReadImages(path, uploadData, info.Slug);
        var imageSet = new ImageSet(info.Slug, info.Title, info.Description, info.Tags, info.lowerYearRange, info.upperYearRange, count);

        uploadData.ImageSets.Add(imageSet);

    }

    private int ReadImages(string path, UploadData uploadData, string imageSetSlug)
    {
        var imageFiles = Directory.EnumerateFiles(path, "*.jpg");
        var counter = 1;
        foreach (var imageFile in imageFiles)
        {
            var imageName = Path.GetFileNameWithoutExtension(imageFile);
            var infoPath = Path.Combine(path, $"{imageName}.json");

            if (!File.Exists(infoPath)) throw new Exception($"No json file found for image {imageName}");
            var imageInfo = JsonSerializer.Deserialize<ImageInfo>(File.ReadAllText(infoPath), _options);
            if (imageInfo == null) throw new Exception($"Could not deserialize json file for image {imageName}");

            var url = _imageUrlService.GetImageUrl(imageSetSlug, counter);
            var image = new Image(imageSetSlug, counter, imageInfo.Year, imageInfo.Latitude, imageInfo.Longitude, imageInfo.Description, imageInfo.License, url);
            uploadData.Images.Add(image);
            uploadData.Files.Add(new ImageContent(imageFile, imageSetSlug, counter));
            counter++;
        }

        return counter;
    }
}