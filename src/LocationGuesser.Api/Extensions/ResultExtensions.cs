using FluentResults;
using LocationGuesser.Api.Contracts;

namespace LocationGuesser.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToErrorResponse(this Result result)
    {
        var body = new
        {
            Errors = result.Errors,
            StatusCode = 500
        };
        return new ErrorResult(body);
    }

    public static IResult ToErrorResponse<T>(this Result<T> result)
    {
        var body = new
        {
            Errors = result.Errors,
            StatusCode = 500
        };
        return new ErrorResult(body);
    }

}