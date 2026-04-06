using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.HttpProviders.Emoney.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

public class OnUsPaymentController : ApiControllerBase
{
    private readonly IOnUsPaymentHttpClient _httpClient;

    public OnUsPaymentController(IOnUsPaymentHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// OnUsPayment Initialization
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "OnUsPayment:Create")]
    [HttpPost("init")]
    public async Task<OnUsPaymentResponse> InitOnUsPaymentAsync([FromBody] InitOnUsRequest request)
    {
        return await _httpClient.InitOnUsPaymentAsync(request);
    }

    /// <summary>
    /// Gets OnUsPayment Request Details
    /// </summary>
    /// <param name="onUsPaymentRequestId"></param>
    [Authorize(Policy = "OnUsPayment:Read")]
    [HttpGet("details")]
    public async Task<OnUsPaymentRequest> GetOnUsPaymentDetailsAsync(Guid onUsPaymentRequestId)
    {
        return await _httpClient.GetOnUsPaymentDetailsAsync(onUsPaymentRequestId);
    }


    /// <summary>
    /// Approve OnUsPayment 
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "OnUsPayment:Update")]
    [HttpPut("approve")]
    public async Task<ProvisionPreviewResponse> ApproveOnUsPaymentAsync([FromBody] OnUsPaymentApproveRequest request)
    {
        return await _httpClient.ApproveOnUsPaymentAsync(request);
    }
}