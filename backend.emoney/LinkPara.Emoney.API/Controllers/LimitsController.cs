using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetUserCurrentLimits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class LimitsController : ApiControllerBase
{
    /// <summary>
    /// Returns account current limits
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Read")]
    [HttpGet("account")]
    public async Task<AccountLimitDto> GetAccountLimitsAsync([FromQuery] GetAccountLimitsQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns user current limits
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Read")]
    [HttpGet("user")]
    public async Task<UserLimitDto> GetUserLimitsAsync([FromQuery] GetUserLimitsQuery request)
    {
        return await Mediator.Send(request);
    }
}