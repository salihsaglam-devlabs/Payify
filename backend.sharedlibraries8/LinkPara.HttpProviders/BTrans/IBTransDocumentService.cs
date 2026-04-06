using LinkPara.HttpProviders.BTrans.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.BTrans;

public interface IBTransDocumentService
{
    Task<PaginatedList<DocumentDto>> GetAllDocumentsAsync(GetDocumentsRequest request);
    Task<ParquetContentDto> GetDocumentContentAsync(Guid id);
}