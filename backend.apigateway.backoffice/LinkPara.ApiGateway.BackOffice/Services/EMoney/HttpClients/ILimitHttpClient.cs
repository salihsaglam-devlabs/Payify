using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface ILimitHttpClient
{
    Task<AccountLimitDto> GetAccountLimitsRequestAsync(GetAccountLimitsRequest request);
}