using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public interface IMerchantReturnPoolHttpClient
    {
        public Task<PaginatedList<MerchantReturnPoolDto>> GetFilterListAsync(GetMerchantReturnPoolsRequest request);
        public Task<ReturnResponse> ActionMerchantReturnPoolAsync(ActionMerchantReturnPoolRequest request);
    }
}
