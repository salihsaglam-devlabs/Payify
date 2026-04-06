using LinkPara.ApiGateway.BackOffice.Services.Document.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Document.HttpClients
{
    public interface IDocumentHttpClient
    {
        Task DeleteDocumentAsync(Guid id);
        Task<DocumentDto> GetDocumentAsync(Guid Id);
        Task<PaginatedList<DocumentResponse>> GetDocumentsAsync(GetDocumentsRequest request);
        Task<DocumentResponse> SaveDocumentAsync(DocumentDto request);
        Task<DocumentResponse> UpdateDocumentAsync(UpdateDocumentDto request);
    }
}