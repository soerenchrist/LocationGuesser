using FluentResults;
using FluentValidation;
using LocationGuesser.Api.Contracts;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

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
                Result<List<ImageSet>> { IsSuccess: true } r => Results.Ok(r.Value),
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
                Result<ImageSet> { IsSuccess: true } r => Results.Ok(r.Value),
                _ => result.ToErrorResponse()
            };
        });

        group.MapPost("/", async (
            [FromBody] CreateImageSetContract imageSetContract,
            [FromServices] IImageSetRepository imageSetRepository,
            [FromServices] IValidator<CreateImageSetContract> validator,
            CancellationToken cancellationToken
        ) =>
        {
            var validationResult = await validator.ValidateAsync(imageSetContract);
            if (!validationResult.IsValid) return validationResult.ToBadRequest();

            var imageSet = imageSetContract.ToImageSet();
            var result = await imageSetRepository.AddImageSetAsync(imageSet, cancellationToken);

            if (result.IsFailed)
            {
                return result.ToErrorResponse();
            }
            else
            {
                return Results.Created($"/api/imagesets/{imageSet.Id}", null);
            }
        });
    }
}