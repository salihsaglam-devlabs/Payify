using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Billing.Application.Features.Sectors.Queries;

public class GetAllSectorQuery : SearchQueryParams, IRequest<PaginatedList<SectorDto>>
{
    public RecordStatus? RecordStatus { get; set; }
}

public class GetSectorListQueryHandler : IRequestHandler<GetAllSectorQuery, PaginatedList<SectorDto>>
{

    private readonly ISectorService _sectorService;

    public GetSectorListQueryHandler(ISectorService sectorService)
    {
        _sectorService = sectorService;
    }

    public async Task<PaginatedList<SectorDto>> Handle(GetAllSectorQuery request, CancellationToken cancellationToken)
    {
        return await _sectorService.GetListAsync(request);
    }
}