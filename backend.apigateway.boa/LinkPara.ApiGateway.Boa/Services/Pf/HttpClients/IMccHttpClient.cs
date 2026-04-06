using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;

public interface IMccHttpClient
{
    Task<PaginatedList<MccDto>> GetAllAsync(GetFilterMccRequest request);
}