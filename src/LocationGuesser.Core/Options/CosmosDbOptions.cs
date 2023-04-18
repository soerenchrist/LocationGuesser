using System.ComponentModel.DataAnnotations;

namespace LocationGuesser.Core.Options;

public class CosmosDbOptions
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;
    [Required]
    public string DatabaseName { get; set; } = string.Empty;
    [Required]
    public string ContainerName { get; set; } = string.Empty;
}