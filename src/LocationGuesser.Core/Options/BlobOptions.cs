using System.ComponentModel.DataAnnotations;

namespace LocationGuesser.Core.Options;

public class BlobOptions
{
    [Required] public string Endpoint { get; set; } = string.Empty;

    [Required] public string ContainerName { get; set; } = string.Empty;
}