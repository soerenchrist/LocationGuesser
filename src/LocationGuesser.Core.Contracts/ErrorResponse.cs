namespace LocationGuesser.Core.Contracts;

public record ErrorResponse(int StatusCode, List<ErrorValue> Errors);

public record ErrorValue(string Message);