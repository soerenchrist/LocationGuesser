using Grpc.Core;
using LocationGuesser.Api.Mappings;
using LocationGuesser.Core.Data.Abstractions;

namespace LocationGuesser.Api.Services;

public class ImageSetGrpcService : ImageSetGrpc.ImageSetGrpcBase
{
    private readonly IImageSetRepository _imageSetRepository;

    public ImageSetGrpcService(IImageSetRepository imageSetRepository)
    {
        _imageSetRepository = imageSetRepository;
    }

    public override async Task<GetImageSetResponse> GetImageSet(GetImageSetRequest request, ServerCallContext context)
    {
        var id = request.Id;
        var guid = Guid.Parse(id);
        var result = await _imageSetRepository.GetImageSetAsync(guid, context.CancellationToken);
        if (result.IsFailed)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Errors.First().ToString() ?? string.Empty));
        }

        return result.Value!.ToResponse();
    }
}