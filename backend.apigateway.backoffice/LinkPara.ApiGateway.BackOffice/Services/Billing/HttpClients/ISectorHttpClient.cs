using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public interface ISectorHttpClient
{
    Task<PaginatedList<SectorDto>> GetAllSectorAsync(SectorFilterRequest request);
}