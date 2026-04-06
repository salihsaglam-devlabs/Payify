using LinkPara.Emoney.Application.Features.WithdrawRequests;
using LinkPara.Emoney.Application.Features.WithdrawRequests.Queries.GetWithdrawRequestById;
using LinkPara.Emoney.Application.Features.WithdrawRequests.Queries.GetWithdrawRequestList;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class WithdrawRequestsController : ApiControllerBase
{
    /// <summary>
    /// Returns withdraw details
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<WithdrawRequestDto>> GetByIdAsync(Guid id)
    {
        return await Mediator.Send(new GetWithdrawRequestByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns withdraw detail list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Transaction:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<WithdrawRequestAdminDto>>> GetWithdrawRequestListAsync(
        [FromQuery] GetWithdrawRequestListQuery request)
    {
        return await Mediator.Send(request);
    }
}