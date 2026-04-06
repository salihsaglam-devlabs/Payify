using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public interface IInstitutionHttpClient
{
    Task<PaginatedList<InstitutionDto>> GetAllInstitutionAsync(InstitutionFilterRequest request);
    Task<InstitutionDto> GetByIdAsync(Guid institutionId);
    Task UpdateAsync(UpdateInstitutionRequest request);
}