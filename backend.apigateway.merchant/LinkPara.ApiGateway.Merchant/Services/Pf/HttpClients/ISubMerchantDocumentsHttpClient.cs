using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface ISubMerchantDocumentsHttpClient
{
    Task<PaginatedList<SubMerchantDocumentDto>> GetAllAsync(GetAllSubMerchantDocumentRequest request);
    Task<SubMerchantDocumentDto> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task SaveSubMerchantDocument(SaveSubMerchantDocumentRequest request);
    Task UpdateSubMerchantDocument(SubMerchantDocumentDto request);
}