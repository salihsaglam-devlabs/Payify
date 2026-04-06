using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Application.Features.Posting;
using LinkPara.PF.Application.Features.Posting.Queries.GetAllPostingPfProfits;
using LinkPara.PF.Application.Features.PostingBalances;
using LinkPara.PF.Application.Features.PostingBalances.Commands;
using LinkPara.PF.Application.Features.PostingBalances.Commands.RetryPayment;
using LinkPara.PF.Application.Features.PostingBalances.Queries.GetAllPostingBalances;
using LinkPara.PF.Application.Features.PostingBalances.Queries.GetPostingBalanceById;
using LinkPara.PF.Application.Features.PostingBalances.Queries.GetPostingBalanceStatistics;
using LinkPara.PF.Application.Features.PostingBalances.Queries.GetPostingBalanceStatus;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class PostingBalancesController : ApiControllerBase
{
    /// <summary>
    /// Returns all posting balance
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PostingBalance:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PostingBalanceResponse>> GetAllAsync([FromQuery] GetAllPostingBalanceQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns a posting balance 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PostingBalance:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PostingBalanceDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetPostingBalanceByIdQuery { Id = id });
    }

    /// <summary>
    /// Return a posting balance status counts
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "PostingBalance:ReadAll")]
    [HttpGet("statusCount")]
    public async Task<List<PostingBalanceStatusModel>> GetStatusCountAsync([FromQuery] GetPostingBalanceStatusQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Updates Posting balance properties with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="postingBalance"></param>
    /// <returns></returns>
    [Authorize(Policy = "PostingBalance:Update")]
    [HttpPatch("{id}")]
    public async Task Patch(Guid id, [FromBody] JsonPatchDocument<PatchPostingBalanceRequest> postingBalance)
    {
        await Mediator.Send(new PatchPostingBalanceCommand { Id = id, PostingBalance = postingBalance });
    }

    /// <summary>
    /// Updates Posting balance properties with patch
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PostingBalance:Update")]
    [HttpPut("retry-payment")]
    public async Task UpdatePaymentAsync(RetryPaymentCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Returns posting balance statistics
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PostingBalance:ReadAll")]
    [HttpGet("statistics")]
    public async Task<ActionResult<PostingBalanceStatisticsResponse>> GetStatisticsAsync([FromQuery] GetPostingBalanceStatisticsQuery request)
    {
        return await Mediator.Send(request);
    }
    
    /// <summary>
    /// Returns posting balance statistics
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PostingBalance:ReadAll")]
    [HttpGet("pf-profits")]
    public async Task<ActionResult<PaginatedList<PostingPfProfitDto>>> GetAllPostingPfProfits([FromQuery] GetAllPostingPfProfitsQuery request)
    {
        return await Mediator.Send(request);
    }
}
