using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.NaceCodes.Queries.GetAllNaceCodes;

public class GetAllNaceCodesQuery : SearchQueryParams, IRequest<PaginatedList<NaceDto>>
{
    public string SectorCode { get; set; }
    public string ProfessionCode { get; set; }
    public string Code { get; set; } 
}

public class GetAllNaceCodesQueryHandler : IRequestHandler<GetAllNaceCodesQuery, PaginatedList<NaceDto>>
{
    private readonly INaceService _naceService;

    public GetAllNaceCodesQueryHandler(INaceService naceService)
    {
        _naceService = naceService;
    }
    public async Task<PaginatedList<NaceDto>> Handle(GetAllNaceCodesQuery request, CancellationToken cancellationToken)
    {
        return await _naceService.GetListAsync(request);
    }
}