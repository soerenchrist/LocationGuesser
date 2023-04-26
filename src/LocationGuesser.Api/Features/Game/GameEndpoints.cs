using MediatR;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using LocationGuesser.Core.Domain;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Core.Contracts;
using LocationGuesser.Api.Mappings;

namespace LocationGuesser.Api.Features.Game;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        app.MapGet("/api/games/daily", async (
            [FromServices] IMediator mediator
        ) =>
        {
            var query = new GetDailyChallengeQuery(DateTime.Today);

            var result = await mediator.Send(query);
            if (result.IsFailed) return result.ToErrorResponse();

            var response = new GameContract(result.Value.ImageSet.ToContract(),
                result.Value.Images.Select(x => x.ToContract()).ToList());
            return Results.Ok(response);
        }).CacheOutput("NoCache");
        app.MapGet("/api/games/{setSlug}", async (
            [FromRoute] string setSlug,
            [FromServices] IMediator mediator,
            [FromQuery] int? imageCount
        ) =>
        {
            var count = imageCount ?? 5;
            var command = new GetGameQuery(setSlug, count);
            var imagesTask = mediator.Send(command);

            var query = new GetImageSetQuery(setSlug);
            var imageSetTask = mediator.Send(query);

            await Task.WhenAll(imagesTask, imageSetTask);

            var images = await imagesTask;
            var imageSet = await imageSetTask;

            if (images.IsFailed)
            {
                return images.ToErrorResponse();
            }

            if (imageSet.IsFailed)
            {
                return imageSet.ToErrorResponse();
            }

            var result = new GameContract(
                imageSet.Value.ToContract(),
                images.Value.Select(x => x.ToContract()).ToList()
            );

            return Results.Ok(result);
        }).CacheOutput("NoCache");
    }
}