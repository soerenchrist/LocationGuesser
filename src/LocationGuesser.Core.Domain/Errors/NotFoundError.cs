using FluentResults;

namespace LocationGuesser.Core.Domain.Errors;

public class NotFoundError : Error
{
    public NotFoundError(string message)
        : base(message)
    {
    }
}