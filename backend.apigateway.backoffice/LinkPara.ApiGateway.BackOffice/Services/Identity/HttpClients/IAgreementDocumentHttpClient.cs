using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public interface IAgreementDocumentHttpClient
{
    Task CreateDocumentAsync(CreateDocumentRequest request);
    Task<AgreementDocumentResponse> GetAgreementDocumentByIdAsync(Guid id);
    Task<PaginatedList<AgreementDocumentResponse>> GetAllAgreementDocumentAsync(GetAllAgreementDocumentRequest request);
    Task UpdateAsync(UpdateAgreementDocumentRequest request);
    Task<PaginatedList<AgreementUserDto>> GetAgreedUsersOfDocument(AgreedUserListRequest request);
    Task DeleteAgreementDocumentAsync(Guid id);
}