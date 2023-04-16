using System.Text.Json.Serialization;

namespace LocationGuesser.Core.Domain;

public record ImageSet(Guid Id, string Title, string Description, string Tags);