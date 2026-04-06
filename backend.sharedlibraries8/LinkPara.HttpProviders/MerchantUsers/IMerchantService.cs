using LinkPara.HttpProviders.MerchantUsers.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.MerchantUsers;

public interface IMerchantService 
{
    Task<GetMerchantUserResponse> GetMerchantUser(Guid userId);
}
