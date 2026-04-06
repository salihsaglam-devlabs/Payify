using LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Billing;

public class BillingController : ApiControllerBase
{
    private readonly IBillingHttpClient _billingHttpClient;

    public BillingController(IBillingHttpClient billingHttpClient)
    {
        _billingHttpClient = billingHttpClient;
    }

    /// <summary>
    /// get billing transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:ReadAll")]
    [HttpGet("bill-transactions")]
    public async Task<PaginatedList<BillTransactionDto>> GetBillTransactionsAsync([FromQuery] BillTransactionFilterRequest request)
    {
        return await _billingHttpClient.GetBillTransactionsAsync(request);
    }

    /// <summary>
    /// get billing transaction by provision conversationId
    /// </summary>
    /// <param name="conversationId"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Read")]
    [HttpGet("bill-info/{conversationId}")]
    public async Task<BillTransactionDto> GetBillInfoByConversationIdAsync([FromRoute] string conversationId)
    {
        return await _billingHttpClient.GetBillInfoByConversationIdAsync(conversationId);

    }

    /// <summary>
    /// cancel bill payment service
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Update")]
    [HttpPut("cancel-bill-payment")]
    public async Task<BillCancelResponseDto> CancelBillPaymentAsync([FromBody] BillCancelRequest request)
    {
        return await _billingHttpClient.CancelBillPaymentAsync(request);
    }

    [Authorize(Policy = "Billing:Update")]
    [HttpPut("cancel-bill-accounting")]
    public async Task<BillCancelAccountingResponseDto> CancelBillAccountingAsync([FromBody] BillCancelAccountingRequest request)
    {
        return await _billingHttpClient.CancelBillAccountingAsync(request);
    }
}