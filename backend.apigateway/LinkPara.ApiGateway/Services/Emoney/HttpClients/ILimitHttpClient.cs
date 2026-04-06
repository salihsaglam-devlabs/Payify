using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface ILimitHttpClient
{
    Task<UserLimitDto> GetUserLimitsAsync(GetUserLimitsQuery request);
    Task<AccountLimitDto> GetAccountLimitsAsync(GetAccountLimitsQuery request);
}