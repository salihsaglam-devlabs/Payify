using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests.AgreementDocuments;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;

public interface IAgreementDocumentHttpClient
{
    Task<List<AgreementDocumentVersionDto>> GetDocumentsAsync();
    Task CreateUserDocumentAsync(CreateDocumentToUserRequest request);
}