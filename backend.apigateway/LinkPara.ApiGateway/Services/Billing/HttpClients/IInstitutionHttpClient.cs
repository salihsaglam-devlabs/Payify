using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Billing.HttpClients;

public interface IInstitutionHttpClient
{
    Task<PaginatedList<InstitutionDto>> GetAllInstitutionAsync(InstitutionFilterRequest request);
}