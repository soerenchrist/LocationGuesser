using FluentResults;
using LocationGuesser.Api.Contracts;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToErrorResponse(this ResultBase result)
    {
        if (result.IsNotFound())
        {
            return result.ToNotFoundResponse();
        }
        var body = new ErrorResponse(500, result.Errors.ToErrorsList());
        return new ErrorResult(body);
    }

    public static List<ErrorValue> ToErrorsList(this List<IError> errors)
    {
        return errors.Select(x => new ErrorValue(x.Message)).ToList();
    }

    public static IResult ToNotFoundResponse(this ResultBase result)
    {
        if (!result.IsNotFound())
            throw new InvalidOperationException("Result is not a NotFoundError");

        var errorValues = new List<ErrorValue>
        {
            new ErrorValue(result.Errors.First(x => x is NotFoundError).Message)
        };
        var errorResponse = new ErrorResponse(404, errorValues);
        return Results.NotFound(errorResponse);
    }
    public static bool IsNotFound(this ResultBase result)
    {
        return result.Errors.Any(x => x is NotFoundError);
    }

}