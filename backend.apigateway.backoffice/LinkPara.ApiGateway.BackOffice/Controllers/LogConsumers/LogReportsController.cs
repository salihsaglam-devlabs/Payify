using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.LogConsumers;

public class LogReportsController : ApiControllerBase
{
    private readonly ILogReportsHtppClient _logReportsHtppClient;

    public LogReportsController(ILogReportsHtppClient logReportsHtppClient)
    {
        _logReportsHtppClient = logReportsHtppClient;
    }

    /// <summary>
    /// Returns all Entity Change Logs.
    /// </summary>
    /// <returns></returns>
    [HttpGet("entity-change-log")]
    [Authorize(Policy = "LogConsumers:ReadAll")]
    public async Task<ActionResult<PaginatedList<EntityChangeLogDto>>> GetEntityChangeLogsAsync([FromQuery] GetFilterEntityChangeLogRequest request)
    {
        return await _logReportsHtppClient.GetEntityChangeLogsAsync(request);
    }
}
