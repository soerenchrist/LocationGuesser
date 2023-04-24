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
        app.MapGet("/api/games/{setSlug}", async (
            [FromRoute] string setSlug,
            [FromServices] IMediator mediator,
            [FromQuery] int? imageCount
        ) =>
        {
            var count = imageCount ?? 5;
            var command = new GetGameQuery(setSlug, count);
            var imagesTask = mediator.Send<Result<List<Image>>>(command);

            var query = new GetImageSetQuery(setSlug);
            var imageSetTask = mediator.Send<Result<ImageSet>>(query);

            await Task.WhenAll(imagesTask, imageSetTask);

            var images = await imagesTask;
            var imageSet = await imageSetTask;

            if (images.IsFailed)
            {
                return images.ToErrorResponse();
            }
            else if (imageSet.IsFailed)
            {
                return imageSet.ToErrorResponse();
            }

            var result = new GetGameResult(
                imageSet.Value.ToContract(),
                images.Value.Select(x => x.ToContract()).ToList()
            );

            return Results.Ok(result);
        });
    }
}

file record GetGameResult(ImageSetContract ImageSet, List<ImageContract> Images);