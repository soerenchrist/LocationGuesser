using LocationGuesser.Api.Features.Game;
using MediatR;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using LocationGuesser.Core.Domain;
using LocationGuesser.Api.Extensions;

namespace LocationGuesser.Api.Endpoints;

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
            var count = imageCount == null ? 5 : imageCount.Value;
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