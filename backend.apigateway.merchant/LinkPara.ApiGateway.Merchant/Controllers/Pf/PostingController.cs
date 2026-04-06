using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class PostingController : ApiControllerBase
{
    private readonly IPostingHttpClient _postingHttpClient;

    public PostingController(IPostingHttpClient postingHttpClient)
    {
        _postingHttpClient = postingHttpClient;
    }

    /// <summary>
    /// Get Posting Bills
    /// </summary>
    /// <param name="request"></param>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [HttpGet("{merchantId}/bills")]
    [Authorize(Policy = "Posting:ReadAll")]
    public async Task<ActionResult<PaginatedList<PostingBillDto>>> GetBillsAsync([FromRoute] Guid merchantId, [FromQuery] GetPostingBillRequest request)
    {
        return await _postingHttpClient.GetBillsAsync(merchantId, request);
    }
}
