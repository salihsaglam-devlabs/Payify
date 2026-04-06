using LinkPara.LogConsumers.Commons.Features.LogReports.EntityChangeLogs.Queries;
using LinkPara.LogConsumers.Commons.Features.LogReports.RequestResponseLogs.Queries;
using LinkPara.LogConsumers.Commons.Models;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.LogConsumers.Controllers;

public class LogReportsController : ApiControllerBase
{

    /// <summary>
    /// Returns filtered user history
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>  
    [HttpGet("entity-change-log")]
    [Authorize(Policy = "LogConsumers:ReadAll")]
    public async Task<ActionResult<PaginatedList<EntityChangeLogDto>>> GetFilterAsync([FromQuery] GetFilterEntityChangeLogQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns filtered request response log
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>  
    [HttpGet("request-response-log")]
    [Authorize(Policy = "LogConsumers:ReadAll")]
    public async Task<ActionResult<PaginatedList<RequestResponseLogCreated>>> GetFilterRequestResponseAsync([FromQuery] GetRequestResponseLogQuery query)
    {
        return await Mediator.Send(query);
    }
}
