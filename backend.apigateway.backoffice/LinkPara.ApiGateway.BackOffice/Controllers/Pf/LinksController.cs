using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class LinksController : ApiControllerBase
{
    private readonly ILinkHttpClient _linkHttpClient;

    public LinksController(ILinkHttpClient linkHttpClient)
    {
        _linkHttpClient = linkHttpClient;

    }
    /// <summary>
    /// Get all links by filter link, transaction and customer info.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Link:ReadAll")]
    [HttpPost("payment-report")]
    public async Task<PaginatedList<LinkDto>> GetAllAsync(GetFilterLinkRequest request)
    {
        return await _linkHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Get link payment detail.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "LinkPayment:Read")]
    [HttpGet("payment-detail")]
    public async Task<PaginatedList<LinkPaymentDetailResponse>> GetLinkPaymentDetailAsync([FromQuery] GetPaymentDetailRequest request)
    {
        return await _linkHttpClient.GetLinkPaymentDetailAsync(request);
    }
}
