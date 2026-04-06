using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Application.Features.Payments;
using LinkPara.PF.Application.Features.Payments.Commands.GetBinInformation;
using LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSession;
using LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSessionResult;
using LinkPara.PF.Application.Features.Payments.Commands.Init3ds;
using LinkPara.PF.Application.Features.Payments.Commands.Inquire;
using LinkPara.PF.Application.Features.Payments.Commands.ManualReturn;
using LinkPara.PF.Application.Features.Payments.Commands.PointInquiry;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Application.Features.Payments.Commands.Return;
using LinkPara.PF.Application.Features.Payments.Commands.Reverse;
using LinkPara.PF.Application.Features.Payments.Commands.Verify3ds;
using LinkPara.PF.Application.Features.Payments.Commands.VerifyOnUsPayment;
using LinkPara.PF.Application.Features.Payments.Queries.GetApiLogs;
using LinkPara.PF.Application.Features.Payments.Queries.GetPaymentDetails;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class PaymentsController : ApiControllerBase
{
    /// <summary>
    /// Payment provision
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("provision")]
    public async Task<ProvisionResponse> ProvisionAsync(ProvisionCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Get ThreeDSession
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("getthreedsession")]
    public async Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Get ThreeDSession Result
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("getthreedsessionresult")]
    public async Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Starts 3ds verification
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("init3ds")]
    public async Task<Init3dsResponse> Init3ds(Init3dsCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Reverse order
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("reverse")]
    public async Task<ReverseResponse> ReverseAsync(ReverseCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Return order
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("return")]
    public async Task<ReturnResponse> ReturnAsync(ReturnCommand request)
    {
        return await Mediator.Send(request);
    }
    
    /// <summary>
    /// Returns a payment detail
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("inquire")]
    public async Task<InquireResponse> InquireAsync(InquireCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns a bin detail
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("bin-information")]
    public async Task<GetBinInformationResponse> GetBinInformationAsync(GetBinInformationCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns a point amount
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("pointInquiry")]
    public async Task<PointInquiryResponse> PointInquiryAsync(PointInquiryCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns a payment api logs
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("logs")]
    public async Task<ActionResult<PaginatedList<ApiLogModel>>> GetApiLogAsync([FromQuery] GetApiLogsQuery query)
    {
        return await Mediator.Send(query);
    }
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("verify3ds")]
    public async Task<Verify3dsResponse> Verify3Ds(Verify3dsCommand request)
    {
        return await Mediator.Send(request);        
    }
    [AllowAnonymous]
    [HttpGet("{orderId}")]
    public async Task<ActionResult<PosPaymentDetailResponse>> GetPaymentDetailByOrderIdAsync([FromRoute] string orderId)
    {
        return await Mediator.Send(new GetPaymentDetailQuery { OrderId = orderId });
    }
    [Authorize(Policy = "OnUsPayment:Update")]
    [HttpPost("verify-onus-payment")]
    public async Task<ProvisionResponse> VerifyOnUsPayment(VerifyOnUsPaymentCommand request)
    {
        return await Mediator.Send(request);        
    }
}