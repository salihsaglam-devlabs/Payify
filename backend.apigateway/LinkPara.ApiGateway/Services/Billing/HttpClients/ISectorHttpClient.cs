using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Billing.HttpClients;

public interface ISectorHttpClient
{
    Task<PaginatedList<SectorDto>> GetAllSectorAsync(SectorFilterRequest request);
}