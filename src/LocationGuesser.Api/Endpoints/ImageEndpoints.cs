using LocationGuesser.Api.Extensions;
using LocationGuesser.Api.Features.Images;
using LocationGuesser.Api.Mappings;
using LocationGuesser.Core.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LocationGuesser.Api.Endpoints;

public static class ImageEndpoints
{
    public static void MapImageEndpoints(this WebApplication app)
    {
        app.MapPost("/api/imagesets/{id:guid}/images", async (
            [FromBody] CreateImageContract contract,
            [FromServices] IMediator mediator) =>
        {
            var command = new CreateImageCommand(
                contract.SetId,
                contract.Description,
                contract.Year,
                contract.Latitude,
                contract.Longitude,
                contract.License);

            var result = await mediator.Send(command);

            if (result.IsFailed)
            {
                return result.ToErrorResponse();
            }

            return Results.Ok(result.Value.ToContract());
        });
    }
}