using LinkPara.Billing.Application.Features.Sectors;
using LinkPara.Billing.Application.Features.Sectors.Queries;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface ISectorService
{
    Task<PaginatedList<SectorDto>> GetListAsync(GetAllSectorQuery request);
}