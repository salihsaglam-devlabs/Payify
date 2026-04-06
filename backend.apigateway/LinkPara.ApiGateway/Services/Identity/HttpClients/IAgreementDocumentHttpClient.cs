using LinkPara.ApiGateway.Services.Identity.Models.Requests.AgreementDocuments;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public interface IAgreementDocumentHttpClient
{
    Task<List<AgreementDocumentVersionDto>> GetDocumentsAsync();
    Task CreateUserDocumentAsync(CreateDocumentToUserRequest request);
}