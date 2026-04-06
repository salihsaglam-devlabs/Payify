using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Command.RetryUnacceptableTransaction;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Command.UpdateUnacceptableTransaction;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Queries.GetAllUnacceptableTransaction;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Queries.GetUnacceptableTransactionById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers.PhysicalPos;

public class UnacceptableTransactionsController : ApiControllerBase
{
    /// <summary>
    /// Retry unacceptable transaction
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosUnacceptable:Create")]
    [HttpPost("retry")]
    public async Task RetryUnacceptableAsync(RetryUnacceptableTransactionCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Get all unacceptable records
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosUnacceptable:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<PhysicalPosUnacceptableTransactionDto>>> GetFilterAsync([FromQuery] GetAllUnacceptableTransactionQuery query)
    {
        return await Mediator.Send(query);
    }
    
    /// <summary>
    /// Get unacceptable record with all related transactions
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosUnacceptable:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<UnacceptableTransactionDetailResponse>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetUnacceptableTransactionByIdQuery { Id = id });
    }

    /// <summary>
    /// Update unacceptable transaction current status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="unacceptableTransaction"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosUnacceptable:Update")]
    [HttpPatch("{id}")]
    public async Task<PhysicalPosUnacceptableTransactionDto> Patch(Guid id, [FromBody] JsonPatchDocument<UpdateUnacceptableTransactionRequest> unacceptableTransaction)
    {
        return await Mediator.Send(new UpdateUnacceptableTransactionCommand { Id = id, UnacceptableTransaction = unacceptableTransaction });
    }
}