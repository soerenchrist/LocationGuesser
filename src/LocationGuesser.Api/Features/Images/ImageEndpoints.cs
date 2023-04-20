using FluentResults;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Api.Features.Images;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LocationGuesser.Api.Features.Images;

public static class ImageEndpoints
{
    public static void MapImageEndpoints(this WebApplication app)
    {
        app.MapGet("/api/imagesets/{id:guid}/images/{number:int}/content", async (
            [FromRoute] Guid id,
            [FromRoute] int number,
            [FromServices] IMediator mediator
        ) =>
        {
            var command = new GetImageContentQuery(id, number);

            var result = await mediator.Send<Result<Stream>>(command);

            if (result.IsFailed)
            {
                return result.ToErrorResponse();
            }

            return Results.File(result.Value, "image/jpeg");
        });
    }
}