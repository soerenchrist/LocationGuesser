using FluentResults;

namespace LocationGuesser.Core.Domain.Errors;

public class ValidationError : Error
{
    public ValidationError(string propertyName, string message) : base(message)
    {
        PropertyName = propertyName;
    }

    public string PropertyName { get; }
}