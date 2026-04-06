using LinkPara.ApiGateway.Merchant.Services.Document.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Document.HttpClients
{
    public interface IDocumentTypeHttpClient
    {
        Task CreateDocumentTypeAsync(DocumentTypeDto request);
        Task DeleteDocumentTypeAsync(Guid id);
        Task<DocumentTypeDto> GetDocumentTypeAsync(Guid id);
        Task<PaginatedList<DocumentTypeDto>> GetDocumentTypesAsync(GetDocumentTypesRequest request);
    }
}
