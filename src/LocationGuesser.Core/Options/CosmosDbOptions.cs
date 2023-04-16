namespace LocationGuesser.Core.Options;

public class CosmosDbOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}