using FluentResults;
using FluentValidation;
using LocationGuesser.Api.Extensions;
using LocationGuesser.Core.Data.Abstractions;
using MediatR;

namespace LocationGuesser.Api.Features.Images;

public record GetImageContentQuery(Guid SetId, int Number) : IRequest<Result<Stream>>;

public class GetImageContentQueryHandler : IRequestHandler<GetImageContentQuery, Result<Stream>>
{
    private readonly IBlobRepository _blobRepository;
    private readonly IValidator<GetImageContentQuery> _validator;
    public GetImageContentQueryHandler(IBlobRepository blobRepository, IValidator<GetImageContentQuery> validator)
    {
        _blobRepository = blobRepository;
        _validator = validator;
    }

    public async Task<Result<Stream>> Handle(GetImageContentQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToResult<Stream>();
        }

        var result = await _blobRepository.DownloadImageAsync($"{request.SetId}/{request.Number}.jpg", cancellationToken);
        return result;
    }
}

public class GetImageContentQueryValidator : AbstractValidator<GetImageContentQuery>
{
    public GetImageContentQueryValidator()
    {
        RuleFor(x => x.Number).GreaterThan(0);
    }
}