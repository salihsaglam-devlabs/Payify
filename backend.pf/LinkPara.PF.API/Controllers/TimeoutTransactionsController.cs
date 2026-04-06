using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.TimeoutTransactions.Queries.GetAllTimeoutTransactions;
using LinkPara.PF.Application.Features.TimeoutTransactions;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.PF.API.Controllers;

public class TimeoutTransactionsController : ApiControllerBase
{
    /// <summary>
    /// Returns all timeout transactions
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "TimeoutTransaction:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<TimeoutTransactionDto>>> GetTimeoutTransactionsAsync([FromQuery] GetAllTimeoutTransactionQuery query)
    {
        return await Mediator.Send(query);
    }
}