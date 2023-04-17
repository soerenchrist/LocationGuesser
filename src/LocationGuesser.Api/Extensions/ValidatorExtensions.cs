using FluentValidation.Results;

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
}