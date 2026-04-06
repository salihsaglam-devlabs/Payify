using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IMerchantUserHttpClient
{
    Task<PaginatedList<MerchantUserDto>> GetAllAsync(GetAllMerchantUserRequest request);
    Task<MerchantUserDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveMerchantUserRequest request);
    Task UpdateAsync(MerchantUserDto request);
    Task<MerchantUserNameModel> GetUserNameAsync(string identifier);
}
