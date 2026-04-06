using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;

public interface IFieldHttpClient
{
    Task<PaginatedList<FieldDto>> GetByInstitutionIdAsync(Guid institutionId);
}