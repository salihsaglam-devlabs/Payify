using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class PfApiLogsController : ApiControllerBase
{
    private readonly IPfApiLogHttpClient _pfApiLogHttpClient;

    public PfApiLogsController(IPfApiLogHttpClient pfApiLogHttpClient)
    {
        _pfApiLogHttpClient = pfApiLogHttpClient;
    }

    /// <summary>
    /// Returns a payment api logs
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "PfApiLog:ReadAll")]
    public async Task<ActionResult<PaginatedList<ApiLogDto>>> GetAllAsync([FromQuery] GetAllApiLogRequest request)
    {
        return await _pfApiLogHttpClient.GetAllAsync(request);
    }
}
