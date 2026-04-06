using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public interface IMerchantHistoryHttpClient
    {
        Task<PaginatedList<MerchantHistoryDto>> GetFilterListAsync(GetFilterMerchantHistoryRequest request);
        Task<PaginatedList<MerchantHistoryDto>> GetAllParameterAsync(GetAllMerchantHistoryRequest request);
    }
}
