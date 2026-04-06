using LinkPara.ApiGateway.BackOffice.Services.BTrans.Models;
using LinkPara.ApiGateway.BackOffice.Services.BTrans.Models.Request;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BTrans.HttpClients;

public interface IBTransDocumentClient
{
    Task<PaginatedList<BtransDocumentDto>> GetAllDocumentsAsync(GetDocumentsRequest request);
    Task<ParquetContentDto> GetDocumentContentAsync(Guid id);
}