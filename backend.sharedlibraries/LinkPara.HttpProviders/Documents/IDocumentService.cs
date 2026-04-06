using LinkPara.HttpProviders.Documents.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.Documents;

public interface IDocumentService
{
    Task<PaginatedList<GetDocumentResponse>> GetDocuments(GetDocumentListRequest request);
    Task<CreateDocumentResponse> CreateDocument(CreateDocumentRequest request);
    Task<GetDocumentDto> GetDocumentById(Guid id);
    Task<List<DocumentTypeDto>> GetDocumentTypesAsync(GetDocumentTypesRequest request);
}
