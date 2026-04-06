using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Billing;
using LinkPara.Billing.Application.Features.Billing.Commands.CancelBillAccounting;
using LinkPara.Billing.Application.Features.Billing.Commands.CancelBillPayment;
using LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;
using LinkPara.Billing.Application.Features.Billing.Queries.GetBillInfoByConversationId;
using LinkPara.Billing.Application.Features.Billing.Queries.GetBillPreview;
using LinkPara.Billing.Application.Features.Billing.Queries.GetBillStatus;
using LinkPara.Billing.Application.Features.Billing.Queries.GetBillTransactions;
using LinkPara.Billing.Application.Features.InstitutionApis.Queries.GetBillInquiry;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Interfaces;
using LinkPara.Billing.Infrastructure.Services.BillingServices;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Billing.API.Controllers;

public class BillingsController : ApiControllerBase
{

    /// <summary>
    /// new bill inquiry service
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:ReadAll")]
    [HttpGet("inquire-bill")]
    public async Task<BillInquiryResponseDto> InquireBillAsync([FromQuery] InquireBillQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// inquired bill payment service
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Create")]
    [HttpPost("pay-inquired-bill")]
    public async Task<BillPaymentResponseDto> PayInquiredBillAsync([FromBody] PayInquiredBillCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// inquire bill status service
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Read")]
    [HttpGet("bill-status")]
    public async Task<BillStatusResponseDto> GetBillStatusAsync([FromQuery] GetBillStatusQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// cancel bill payment service
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Update")]
    [HttpPut("cancel-bill-payment")]
    public async Task<BillCancelResponseDto> CancelBillPaymentAsync([FromBody] CancelBillPaymentCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// get billing transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:ReadAll")]
    [HttpGet("bill-transactions")]
    public async Task<PaginatedList<BillTransactionResponseDto>> GetBillTransactionsAsync([FromQuery] GetBillTransactionsQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// get billing transaction by provision conversationId
    /// </summary>
    /// <param name="conversationId"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Read")]
    [HttpGet("bill-info/{conversationId}")]
    public async Task<BillTransactionResponseDto> GetBillInfoByConversationIdAsync([FromRoute] string conversationId)
    {
        return await Mediator.Send(new GetBillInfoByConversationIdQuery { ConversationId = conversationId });
    }

    /// <summary>
    /// cancel accounting operations for given billing transaction
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Update")]
    [HttpPut("cancel-bill-accounting")]
    public async Task<BillCancelAccountingResponseDto> CancelBillAccountingAsync([FromBody] CancelBillAccountingCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Bill request preview
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Read")]
    [HttpPost("bill-preview")]
    public async Task<BillPreviewResponseDto> BillPreviewAsync([FromBody] BillPreviewQuery query)
    {
        return await Mediator.Send(query);
    }
    
}
