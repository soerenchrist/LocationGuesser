using FluentResults;
using FluentValidation;
using LocationGuesser.Api.Contracts;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
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
            var result = await mediator.Send<Result<List<ImageSet>>>(new ListImageSetsQuery(), cancellationToken);
            if (result.IsSuccess)
            {
                var contracts = result.Value.Select(ImageSetContract.FromImageSet);
                return Results.Ok(contracts);
            }
            else
            {
                return result.ToErrorResponse();
            }
        }).WithName("GetImagesets").WithOpenApi();

        group.MapGet("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            var query = new GetImageSetQuery(id);
            var result = await mediator.Send<Result<ImageSet>>(query);
            if (result.IsFailed) return result.ToErrorResponse();
            var contract = ImageSetContract.FromImageSet(result.Value);
            return Results.Ok(contract);
        });

        group.MapPost("/", async (
            [FromBody] CreateImageSetContract imageSetContract,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await mediator.Send<Result<ImageSet>>(imageSetContract, cancellationToken);

            if (result.IsFailed)
            {
                return result.ToErrorResponse();
            }

            var contract = ImageSetContract.FromImageSet(result.Value);

            return Results.Created($"/api/imagesets/{result.Value.Id}", contract);
        });

        group.MapDelete("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken) =>
        {

            var result = await mediator.Send<Result>(new DeleteImageSetCommand(id), cancellationToken);
            if (result.IsFailed)
            {
                return result.ToErrorResponse();
            }

            return Results.NoContent();
        });
    }
}