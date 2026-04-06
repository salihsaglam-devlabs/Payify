using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class LimitsController : ApiControllerBase
{
    private readonly ILimitHttpClient _limitHttpClient;

    public LimitsController(ILimitHttpClient limitHttpClient)
    {
        _limitHttpClient = limitHttpClient;
    }

    /// <summary>
    /// Returns account current limits.
    /// </summary>
    /// <returns></returns>
    [HttpGet("account-limits")]
    [Authorize(Policy = "Limit:Read")]
    public async Task<AccountLimitDto> GetUserLimitsAsync([FromQuery] GetAccountLimitsRequest request)
    {
        return await _limitHttpClient.GetAccountLimitsRequestAsync(request);
    }
}