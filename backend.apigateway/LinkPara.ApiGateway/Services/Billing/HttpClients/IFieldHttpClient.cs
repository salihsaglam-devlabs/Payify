using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Billing.HttpClients;

public interface IFieldHttpClient
{
    Task<PaginatedList<FieldDto>> GetByInstitutionIdAsync(Guid institutionId);
}