using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.LinkPayments.Commands.SaveLinkPayment;
using LinkPara.PF.Application.Features.LinkPayments;
using LinkPara.PF.Application.Features.LinkPayments.Queries.GetLinkByUrlPath;
using LinkPara.PF.Application.Features.LinkPayments.Queries.GetPaymentDetail;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.API.Controllers;

public class LinkPaymentController : ApiControllerBase
{
    /// <summary>
    /// Create a link payment.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "LinkPayment:Create")]
    [HttpPost("")]
    public async Task<LinkPaymentResponse> SaveAsync(SaveLinkPaymentCommand command)
    {
        return await Mediator.Send(command);
    }
    /// <summary>
    /// Get a link payment.
    /// </summary>
    /// <param name="urlPath"></param>
    /// <returns></returns>
    [Authorize(Policy = "LinkPayment:Read")]
    [HttpGet("{urlPath}")]
    public async Task<LinkPaymentPageResponse> GetLinkByUrlPath([FromRoute] string urlPath)
    {
        return await Mediator.Send(new GetLinkByUrlPathQuery() { LinkCode = urlPath});
    }

    /// <summary>
    /// Get a link payment.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "LinkPayment:Read")]
    [HttpGet("detail")]
    public async Task<PaginatedList<LinkPaymentDetailResponse>> GetLinkPaymentDetailAsync([FromQuery] GetPaymentDetailQuery request)
    {
        return await Mediator.Send(request);
    }
}
