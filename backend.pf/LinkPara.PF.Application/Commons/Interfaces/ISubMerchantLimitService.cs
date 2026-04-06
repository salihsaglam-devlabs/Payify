using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchantLimits.Commands.SaveSubMerchantLimit;
using LinkPara.PF.Application.Features.SubMerchantLimits.Commands.UpdateSubMerchantLimit;
using LinkPara.PF.Application.Features.SubMerchantLimits.Queries.GetAllSubMerchantLimits;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ISubMerchantLimitService
{
    Task<PaginatedList<SubMerchantLimitDto>> GetListAsync(GetAllSubMerchantLimitsQuery request);
    Task<SubMerchantLimitDto> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task SaveAsync(SaveSubMerchantLimitCommand request);
    Task UpdateAsync(UpdateSubMerchantLimitCommand request);
}