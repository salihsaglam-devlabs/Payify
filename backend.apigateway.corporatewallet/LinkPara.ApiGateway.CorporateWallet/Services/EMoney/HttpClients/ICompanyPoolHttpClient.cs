using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public interface ICompanyPoolHttpClient
{
    Task<List<CompanyDocumentTypeResponse>> GetCompanyDocumentTypesAsync(GetCompanyDocumentTypesRequest request);
    Task<List<TaxAdministrationsResponse>> GetTaxAdministrationsAsync();
    Task<SaveCompanyPoolResponse> SaveCompanyPoolAsync(SaveCompanyPoolRequest request);
}
