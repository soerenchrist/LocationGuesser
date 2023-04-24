using LocationGuesser.Api.Extensions;
using LocationGuesser.Api.Mappings;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace LocationGuesser.Api.Features.ImageSets;

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
        }).WithName("GetImagesets")
        .WithOpenApi()
        .CacheOutput();
    }
}