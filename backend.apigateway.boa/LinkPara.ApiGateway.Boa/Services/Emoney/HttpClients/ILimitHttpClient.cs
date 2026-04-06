using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public interface ILimitHttpClient
{
    Task<UserLimitDto> GetUserLimitsAsync(GetUserLimitsQuery request);
}