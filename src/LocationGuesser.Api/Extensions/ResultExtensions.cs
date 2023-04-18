using FluentResults;
using LocationGuesser.Api.Contracts;
using LocationGuesser.Api.Util;
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
        if (result.IsValidationError())
        {
            return result.ToValidationErrorResponse();
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

    public static IResult ToValidationErrorResponse(this ResultBase result)
    {
        if (!result.IsValidationError())
            throw new InvalidOperationException("Result is not a ValidationError");

        var errors = result.Errors.Where(x => x is ValidationError)
            .Select(x => new ErrorValue(x.Message))
            .ToList();
        var errorResponse = new ErrorResponse(400, errors);
        return Results.BadRequest(errorResponse);
    }

    public static bool IsNotFound(this ResultBase result)
    {
        return result.Errors.Any(x => x is NotFoundError);
    }

    private static bool IsValidationError(this ResultBase result)
    {
        return result.Errors.Any(x => x is ValidationError);
    }

}