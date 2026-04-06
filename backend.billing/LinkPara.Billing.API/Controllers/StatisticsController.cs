using LinkPara.Billing.Application.Features.Statistics;
using LinkPara.Billing.Application.Features.Statistics.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Billing.API.Controllers;

public class StatisticsController : ApiControllerBase
{
    /// <summary>
    /// Returns statistics for backoffice control panel
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Statistic:Read")]
    [HttpGet("")]
    public async Task<ReportDto> GetReportAsync([FromQuery] GetReportQuery query)
    {
        return await Mediator.Send(query);
    }
}
