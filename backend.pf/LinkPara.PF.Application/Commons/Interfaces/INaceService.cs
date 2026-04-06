using LinkPara.PF.Application.Features.NaceCodes;
using LinkPara.PF.Application.Features.NaceCodes.Queries.GetAllNaceCodes;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface INaceService
{
    Task<PaginatedList<NaceDto>> GetListAsync(GetAllNaceCodesQuery request);
    Task<NaceDto> GetByIdAsync(Guid id);
}