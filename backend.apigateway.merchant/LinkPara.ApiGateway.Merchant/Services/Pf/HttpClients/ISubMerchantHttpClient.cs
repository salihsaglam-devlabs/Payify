using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface ISubMerchantHttpClient
{
    Task<PaginatedList<SubMerchantDto>> GetAllAsync(GetAllSubMerchantRequest request);
    Task<SubMerchantDto> GetByIdAsync(Guid id);
    Task<SubMerchantSummaryDto> GetSubMerchantSummary();
    Task DeleteAsync(Guid id);
    Task<Guid> SaveAsync(SaveSubMerchantRequest request);
    Task UpdateAsync(UpdateSubMerchantRequest request);
    Task ApproveAsync(ApproveSubMerchantRequest request);
    Task UpdateMultipleAsync(UpdateMultipleSubMerchantRequest request);

}
