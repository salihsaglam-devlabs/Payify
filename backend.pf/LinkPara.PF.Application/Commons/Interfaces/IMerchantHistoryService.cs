using LinkPara.PF.Application.Features.MerchantHistory;
using LinkPara.PF.Application.Features.MerchantHistory.Queries.GetAllMerchantHistory;
using LinkPara.PF.Application.Features.MerchantHistory.Queries.GetFilterMerchantHistory;
using LinkPara.SharedModels.Pagination;


namespace LinkPara.PF.Application.Commons.Interfaces
{
    public interface IMerchantHistoryService
    {
        Task<PaginatedList<MerchantHistoryDto>> GetAllMerchantHistoryAsync(GetAllMerchantHistoryQuery request);
        Task<PaginatedList<MerchantHistoryDto>> GetFilterListAsync(GetFilterMerchantHistoryQuery request);
    }
}
