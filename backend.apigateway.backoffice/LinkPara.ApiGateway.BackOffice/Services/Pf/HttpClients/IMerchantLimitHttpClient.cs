using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public interface IMerchantLimitHttpClient
    {
        Task<PaginatedList<MerchantLimitDto>> GetFilterListAsync(GetFilterMerchantLimitRequest request);
        Task SaveAsync(SaveMerchantLimitRequest request);
        Task UpdateAsync(UpdateMerchantLimitRequest request);
    }
}
