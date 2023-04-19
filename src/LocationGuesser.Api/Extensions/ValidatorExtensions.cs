using FluentResults;
using FluentValidation.Results;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Api.Extensions;

public static class ValidatorExtensions
{
    public static Result<T> ToResult<T>(this ValidationResult validationResult)
    {
        if (validationResult.IsValid) return Result.Ok();
        var errors = validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList();
        return Result.Fail<T>(errors);
    }
}