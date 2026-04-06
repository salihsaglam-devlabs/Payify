using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IParentMerchantHttpClient
{
    Task<MerchantDto> GetByIdAsync(Guid id);
    Task<PaginatedList<MerchantDto>> GetFilterListAsync(GetAllParentMerchantRequest request);
}
