using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface ISubMerchantLimitsHttpClient
{
    Task<PaginatedList<SubMerchantLimitDto>> GetAllAsync(GetAllSubMerchantLimitsRequest request);
    Task<SubMerchantLimitDto> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task SaveSubMerchantLimit(SaveSubMerchantLimitRequest request);
    Task UpdateSubMerchantLimit(SubMerchantLimitDto request);
}