using LocationGuesser.Api.Extensions;
using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Api.Mappings;
using LocationGuesser.Core.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LocationGuesser.Api.Endpoints;

public static class ImageSetEndpoints
{
    public static void MapImageSetEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/imagesets");

        group.MapGet("/", async (
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await mediator.Send(new ListImageSetsQuery(), cancellationToken);
            if (result.IsSuccess)
            {
                var contracts = result.Value.Select(x => x.ToContract());
                return Results.Ok(contracts);
            }

            return result.ToErrorResponse();
        }).WithName("GetImagesets").WithOpenApi();

        group.MapGet("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            var query = new GetImageSetQuery(id);
            var result = await mediator.Send(query);
            if (result.IsFailed) return result.ToErrorResponse();
            var contract = result.Value.ToContract();
            return Results.Ok(contract);
        });

        group.MapPost("/", async (
            [FromBody] CreateImageSetContract request,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            var command = new CreateImageSetCommand(request.Title, request.Description, request.Tags,
                request.LowerYearRange, request.UpperYearRange);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsFailed) return result.ToErrorResponse();

            var contract = result.Value.ToContract();

            return Results.Created($"/api/imagesets/{result.Value.Id}", contract);
        });

        group.MapDelete("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new DeleteImageSetCommand(id), cancellationToken);
            if (result.IsFailed) return result.ToErrorResponse();

            return Results.NoContent();
        });
    }
}