using LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Billing;

public class BillingController : ApiControllerBase
{
    private readonly IBillingHttpClient _billingHttpClient;

    public BillingController(IBillingHttpClient billingHttpClient)
    {
        _billingHttpClient = billingHttpClient;
    }

    /// <summary>
    /// inquire bill service
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:ReadAll")]
    [HttpGet("inquire-bill")]
    public async Task<BillInquiryResponseDto> InquireBillAsync([FromQuery] BillInquiryRequest request)
    {
        return await _billingHttpClient.InquireBillAsync(request);
    }

    /// <summary>
    /// inquired bill payment service
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Create")]
    [HttpPost("pay-inquired-bill")]
    public async Task<BillPaymentResponseDto> PayInquiredBillAsync([FromBody] BillPaymentRequest request)
    {
        return await _billingHttpClient.PayInquiredBillAsync(request,Guid.Parse(UserId));
    }

    /// <summary>
    /// cancel bill payment service
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Update")]
    [HttpPut("cancel-bill-payment")]
    public async Task<BillCancelResponseDto> CancelBillPayment([FromBody] BillCancelRequest request)
    {
        return await _billingHttpClient.CancelBillPaymentAsync(request);
    }

    /// <summary>
    /// bill request preview
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Read")]
    [HttpPost("bill-preview")]
    public async Task<BillPreviewResponseDto> BillPreviewAsync([FromBody] BillPreviewRequest request)
    {
        return await _billingHttpClient.BillPreviewAsync(request, Guid.Parse(UserId));
    }
}