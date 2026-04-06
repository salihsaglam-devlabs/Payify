using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class HostedPaymentController : ApiControllerBase
{
    private readonly IHostedPaymentHttpClient _hostedPaymentHttpClient;

    public HostedPaymentController(IHostedPaymentHttpClient hostedPaymentHttpClient)
    {
        _hostedPaymentHttpClient = hostedPaymentHttpClient;
    }
    
    /// <summary>
    /// Get hpp transaction by tracking id.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "HostedPayment:Read")]
    [HttpGet("inquire")]
    public async Task<HppTransactionResponse> GetHppTransactionByTrackingIdAsync([FromQuery] GetHppTransactionByTrackingIdRequest request)
    {
        return await _hostedPaymentHttpClient.GetHppTransactionByTrackingIdAsync(request);
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
        await _hostedPaymentHttpClient.HostedPaymentWebhookTriggerAsync(trackingId);
    }
    
    /// <summary>
    /// Get all hpp transactions.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "HostedPayment:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<HostedPaymentDto>> GetHostedPaymentTransactionsAsync([FromQuery] GetHppTransactionsRequest request)
    {
        return await _hostedPaymentHttpClient.GetHostedPaymentTransactionsAsync(request);
    }
}