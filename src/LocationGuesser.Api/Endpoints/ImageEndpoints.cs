using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LocationGuesser.Api.Endpoints;

public static class ImageEndpoints
{
    public static void MapImageEndpoints(WebApplication app)
    {
        app.MapGet("/api/imagesets/{id:guid}/images", async (
            [FromRoute] Guid id,
            [FromServices] IMediator mediator) =>
        {
            
        });
    }
}