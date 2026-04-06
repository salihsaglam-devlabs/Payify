using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;

public interface IMerchantIntegratorHttpClient
{
    Task<PaginatedList<MerchantIntegratorDto>> GetAllAsync(SearchQueryParams request);
}