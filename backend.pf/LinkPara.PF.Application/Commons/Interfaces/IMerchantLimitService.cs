using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.MerchantLimits.Command.SaveMerchantLimit;
using LinkPara.PF.Application.Features.MerchantLimits.Command.UpdateMerchantLimit;
using LinkPara.PF.Application.Features.MerchantLimits.Queries.GetFilterMerchantLimits;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces
{
    public interface IMerchantLimitService
    {
        Task<PaginatedList<MerchantLimitDto>> GetFilterListAsync(GetFilterMerchantLimitsQuery request);
        Task SaveAsync(SaveMerchantLimitCommand request);
        Task UpdateAsync(UpdateMerchantLimitCommand request);
    }
}
