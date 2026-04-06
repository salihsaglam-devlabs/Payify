using LinkPara.PF.Application.Features.HostedPayments;
using LinkPara.PF.Application.Features.HostedPayments.Command.InitHostedPayment;
using LinkPara.PF.Application.Features.HostedPayments.Command.SaveHostedPayment;
using LinkPara.PF.Application.Features.HostedPayments.Command.TriggerHppWebhook;
using LinkPara.PF.Application.Features.HostedPayments.Queries.GetHppDetailsByTrackingId;
using LinkPara.PF.Application.Features.HostedPayments.Queries.GetHppTransactionByTrackingId;
using LinkPara.PF.Application.Features.HostedPayments.Queries.GetHppTransactions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class HostedPaymentController : ApiControllerBase
{
    /// <summary>
    /// Initiate hosted payment.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "HostedPayment:Create")]
    [HttpPost("init")]
    public async Task<InitHostedPaymentResponse> InitHostedPaymentAsync(InitHostedPaymentCommand command)
    {
        return await Mediator.Send(command);
    }
    
    /// <summary>
    /// Get hpp details by tracking id.
    /// </summary>
    /// <param name="trackingId"></param>
    /// <returns></returns>
    [Authorize(Policy = "HostedPayment:Read")]
    [HttpGet("{trackingId}")]
    public async Task<HostedPaymentPageResponse> GetHppDetailsByTrackingIdAsync([FromRoute] string trackingId)
    {
        return await Mediator.Send(new GetHppDetailsByTrackingIdQuery { TrackingId = trackingId});
    }
    
    /// <summary>
    /// Create a hosted payment.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "HostedPayment:Create")]
    [HttpPost("provision")]
    public async Task<HostedPaymentResponse> HostedPaymentProvisionAsync(SaveHostedPaymentCommand command)
    {
        return await Mediator.Send(command);
    }
    
    /// <summary>
    /// Get hpp transaction by tracking id.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "HostedPayment:Read")]
    [HttpGet("inquire")]
    public async Task<HppTransactionResponse> GetHppTransactionByTrackingId([FromQuery] GetHppTransactionByTrackingIdQuery query)
    {
        return await Mediator.Send(query);
    }
    
    /// <summary>
    /// Manually trigger webhook notification.
    /// </summary>
    /// <param name="trackingId"></param>
    /// <returns></returns>
    [Authorize(Policy = "HostedPayment:Read")]
    [HttpGet("webhook-trigger/{trackingId}")]
    public async Task HostedPaymentWebhookTriggerAsync([FromRoute] string trackingId)
    {
        await Mediator.Send(new TriggerHppWebhookCommand { TrackingId = trackingId});
    }
    
    /// <summary>
    /// Get all hpp transactions.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "HostedPayment:ReadAll")]
    [HttpGet("transactions")]
    public async Task<PaginatedList<HostedPaymentDto>> GetHostedPaymentTransactionsAsync([FromQuery] GetHppTransactionsQuery query)
    {
        return await Mediator.Send(query);
    }
}