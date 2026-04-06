using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class PostingBalancesController : ApiControllerBase
{
    private readonly IPostingBalanceHttpClient _postingBalanceHttpClient;

    public PostingBalancesController(IPostingBalanceHttpClient postingBalanceHttpClient)
    {
        _postingBalanceHttpClient = postingBalanceHttpClient;
    }

    /// <summary>
    ///  Returns all posting balance
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "PostingBalance:ReadAll")]
    public async Task<ActionResult<PostingBalanceResponse>> GetAllAsync([FromQuery] GetAllPostingBalanceRequest request)
    {
        return await _postingBalanceHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a posting balance
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "PostingBalance:Read")]
    public async Task<ActionResult<PostingBalanceDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _postingBalanceHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Return a posting balance status counts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("statusCount")]
    [Authorize(Policy = "PostingBalance:ReadAll")]
    public async Task<ActionResult<List<PostingBalanceStatusModel>>> GetStatusCountAsync([FromQuery] PostingBalanceStatusRequest request)
    {
        return await _postingBalanceHttpClient.GetStatusCountAsync(request);
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
        await _postingBalanceHttpClient.PatchAsync(id, postingBalance);
    }
}
