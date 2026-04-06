using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public interface ILimitHttpClient
{
    Task<UserLimitDto> GetUserLimitsAsync(GetUserLimitsQuery request);
    Task<AccountLimitDto> GetAccountLimitsAsync(GetAccountLimitsQuery request);
}