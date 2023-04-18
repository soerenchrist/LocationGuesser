using FluentResults;
using FluentValidation;
using LocationGuesser.Api.Contracts;
using LocationGuesser.Api.Extensions;
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
            [FromServices] IImageSetRepository imageSetRepository,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await imageSetRepository.ListImageSetsAsync(cancellationToken);
            return result switch
            {
                { IsSuccess: true, Value: null } => Results.NotFound(),
                { IsSuccess: true } r => Results.Ok(r.Value),
                _ => Results.StatusCode(500)
            };
        }).WithName("GetImagesets").WithOpenApi();

        group.MapGet("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IImageSetRepository imageSetRepository,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await imageSetRepository.GetImageSetAsync(id, cancellationToken);
            return result switch
            {
                { IsSuccess: true, Value: null } => Results.NotFound(),
                { IsSuccess: true } r => Results.Ok(r.Value),
                _ => result.ToErrorResponse()
            };
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

            return Results.Created($"/api/imagesets/{result.Value.Id}", result.Value);
        });

        group.MapDelete("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IImageSetRepository imageSetRepository,
            CancellationToken cancellationToken) =>
        {
            var result = await imageSetRepository.GetImageSetAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                return result.ToErrorResponse();
            }

            return Results.NoContent();
        });
    }
}