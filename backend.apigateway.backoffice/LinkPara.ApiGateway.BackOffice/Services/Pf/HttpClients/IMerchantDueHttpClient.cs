using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMerchantDueHttpClient
{
    Task<MerchantDueDto> GetByIdAsync(Guid id);
    Task<PaginatedList<MerchantDueDto>> GetAllMerchantDuesAsync(GetAllMerchantDueRequest request);
    Task SaveMerchantDueAsync(SaveMerchantDueRequest request);
    Task DeleteMerchantDueAsync(Guid id);
}