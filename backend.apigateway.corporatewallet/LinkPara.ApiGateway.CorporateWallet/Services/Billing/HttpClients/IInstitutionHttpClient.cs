using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;

public interface IInstitutionHttpClient
{
    Task<PaginatedList<InstitutionDto>> GetAllInstitutionAsync(InstitutionFilterRequest request);
}