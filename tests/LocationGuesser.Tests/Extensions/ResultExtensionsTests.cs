using FluentResults;
using LocationGuesser.Api.Contracts;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Domain.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LocationGuesser.Tests.Extensions;

public class ResultExtensionsTests
{
    [Fact]
    public void IsNotFound_ShouldReturnFalse_WhenResultIsOk()
    {
        var result = Result.Ok();

        result.IsNotFound().Should().BeFalse();
    }

    [Fact]
    public void IsNotFound_ShouldReturnFalse_WhenResultIsGenericError()
    {
        var result = Result.Fail("Something went wrong");

        result.IsNotFound().Should().BeFalse();
    }

    [Fact]
    public void IsNotFound_ShouldReturnTrue_WhenResultContainsOnlyNotFoundError()
    {
        var result = Result.Fail(new NotFoundError("Not found"));

        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public void IsNotFound_ShouldReturnTrue_WhenResultContainsOneNotFoundError()
    {
        var result = Result.Fail("Something1")
            .WithError("Something 2")
            .WithError(new NotFoundError("Not found"))
            .WithError("Something 3");

        result.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public void ToNotFoundResponse_ShouldThrowInvalidOperationException_WhenResultIsNoNotFoundError()
    {
        var result = Result.Fail("Something");

        var action = () => result.ToNotFoundResponse();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToNotFoundResponse_ShouldMapToErrorResponse()
    {
        var result = Result.Fail(new NotFoundError("Not Found!"));

        var response = (NotFound<ErrorResponse>)result.ToNotFoundResponse();

        response.StatusCode.Should().Be(404);
        response.Value.Should().BeEquivalentTo(new ErrorResponse(404, new List<ErrorValue>{
            new ErrorValue("Not Found!"),
        }));
    }

    [Fact]
    public void ToErrorsList_ShouldMapErrorToErroValues()
    {
        var result = Result.Fail("Error1")
            .WithError("Error2")
            .WithError("Error3");

        var response = result.Errors.ToErrorsList();

        response.Should().BeEquivalentTo(new List<ErrorValue>{
            new ErrorValue("Error1"),
            new ErrorValue("Error2"),
            new ErrorValue("Error3"),
        });
    }

    [Fact]
    public void ToErrorResponse_ShouldReturnNotFoundResponse_WhenResultContainsNotFoundError()
    {
        var result = Result.Fail("Error1")
            .WithError(new NotFoundError("Error2"))
            .WithError("Error3");

        var response = result.ToErrorResponse();

        response.Should().BeOfType<NotFound<ErrorResponse>>();
    }

    [Fact]
    public void ToErrorResponse_ShouldReturnInternalServerError_WhenResultIsUnknownError()
    {
        var result = Result.Fail("Error1")
            .WithError("Error2")
            .WithError("Error3");

        var response = result.ToErrorResponse();

        response.Should().BeOfType<ErrorResult>();
    }

    [Fact]
    public void ToErrorResponse_ShouldReturnBadRequest_WhenResultIsValidationError()
    {
        var result = Result.Fail(new ValidationError("Property", "Failed to validate"));

        var response = result.ToErrorResponse();

        response.Should().BeOfType<BadRequest<ErrorResponse>>();
    }

    private HttpContext CreateHttpContext()
    {
        return Substitute.For<HttpContext>();
    }
}