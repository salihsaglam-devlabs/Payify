using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class TimeoutTransactionsController : ApiControllerBase
{
    private readonly ITimeoutTransactionHttpClient _timeoutTransactionHttpClient;

    public TimeoutTransactionsController(ITimeoutTransactionHttpClient timeoutTransactionHttpClient)
    {
        _timeoutTransactionHttpClient = timeoutTransactionHttpClient;
    }

    /// <summary>
    /// Returns all timeout transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "TimeoutTransaction:ReadAll")]
    public async Task<ActionResult<PaginatedList<TimeoutTransactionDto>>> GetAllAsync([FromQuery] GetFilterTransactionTimeoutRequest request)
    {
        return await _timeoutTransactionHttpClient.GetAllAsync(request);
    }
}
