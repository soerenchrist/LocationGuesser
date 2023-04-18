using FluentResults;
using FluentValidation.Results;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Api.Extensions;

public static class ValidatorExtensions
{
    public static IResult ToBadRequest(this ValidationResult validationResult)
    {
        var response = new {
            Errors = validationResult.Errors,
            StatusCode = 400,
        };
        return Results.BadRequest(response);
    }

    public static Result ToResult<T>(this ValidationResult validationResult)
    {
        if (validationResult.IsValid) return Result.Ok();
        var errors = validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList();
        return Result.Fail(errors);
    }
}