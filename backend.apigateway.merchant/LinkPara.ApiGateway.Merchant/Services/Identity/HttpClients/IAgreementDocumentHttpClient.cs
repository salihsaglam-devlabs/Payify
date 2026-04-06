using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;

public interface IAgreementDocumentHttpClient
{
    Task<List<AgreementDocumentVersionDto>> GetDocumentsAsync();
    Task CreateUserDocumentAsync(CreateDocumentToUserRequest request);
}