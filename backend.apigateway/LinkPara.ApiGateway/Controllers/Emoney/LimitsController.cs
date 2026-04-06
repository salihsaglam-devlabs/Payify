using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

public class LimitsController : ApiControllerBase
{
    private readonly ILimitHttpClient _httpClient;

    public LimitsController(ILimitHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Returns user current limits
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:ReadAll")]
    [HttpGet("user-limits")]
    public async Task<UserLimitDto> GetUserLimitsAsync([FromQuery] string currencyCode)
    {
        return await _httpClient.GetUserLimitsAsync(new GetUserLimitsQuery
        {
            CurrencyCode = currencyCode,
            UserId = Guid.Parse(UserId)
        });
    }

    /// <summary>
    /// Returns account's current limits
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:ReadAll")]
    [HttpGet("account-limits")]
    public async Task<AccountLimitDto> GetAccountLimitsAsync([FromQuery] GetAccountLimitsQuery request)
    {
        return await _httpClient.GetAccountLimitsAsync(request);
    }
}