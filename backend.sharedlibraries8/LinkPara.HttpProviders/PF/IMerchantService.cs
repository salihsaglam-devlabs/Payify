using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.HttpProviders.PF.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.PF
{
    public interface IMerchantService
    {
        Task UpdateMerchantIKSAsync(UpdateMerchantIKSModel request);
        Task<PaginatedList<MerchantDto>> GetFilterListAsync(GetFilterMerchantsRequest request);
    }
}
