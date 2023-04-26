using FluentResults;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Core.Extensions;

public static class ResultExtensions
{
    public static bool IsNotFound(this ResultBase result)
    {
        return result.Errors.Any(x => x is NotFoundError);
    }

    public static bool IsValidationError(this ResultBase result)
    {
        return result.Errors.Any(x => x is ValidationError);
    }
}