using LinkPara.ApiGateway.Boa.Filters.CustomerContext;
using LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Emoney;

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
    [HttpGet("user-limits")]
    [CustomerContextRequired]
    public async Task<UserLimitDto> GetUserLimitsAsync([FromQuery] string currencyCode)
    {
        return await _httpClient.GetUserLimitsAsync(new GetUserLimitsQuery
        {
            CurrencyCode = currencyCode,
            UserId = Guid.Parse(UserId)
        });
    }
}
