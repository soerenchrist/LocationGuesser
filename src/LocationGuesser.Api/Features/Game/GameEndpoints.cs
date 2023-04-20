using MediatR;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using LocationGuesser.Core.Domain;
using LocationGuesser.Api.Extensions;

namespace LocationGuesser.Api.Features.Game;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        app.MapGet("/api/games/{setId:guid}", async (
            [FromRoute] Guid setId,
            [FromServices] IMediator mediator,
            [FromQuery] int? imageCount
        ) =>
        {
            var count = imageCount ?? 5;
            var command = new GetGameQuery(setId, count);
            var result = await mediator.Send<Result<List<Image>>>(command);

            if (result.IsFailed)
            {
                return result.ToErrorResponse();
            }

            return Results.Ok(result.Value);
        });
    }
}