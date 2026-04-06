using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.NaceCodes.Queries.GetNaceCodeById;

public class GetNaceCodeById : IRequest<NaceDto>
{
    public Guid NaceCodeId { get; set; }
}

public class GetNaceCodeByIdHandler : IRequestHandler<GetNaceCodeById, NaceDto>
{
    private readonly INaceService _naceService;

    public GetNaceCodeByIdHandler(INaceService naceService)
    {
        _naceService = naceService;
    }
    
    public async Task<NaceDto> Handle(GetNaceCodeById request, CancellationToken cancellationToken)
    {
        return await _naceService.GetByIdAsync(request.NaceCodeId);
    }
}