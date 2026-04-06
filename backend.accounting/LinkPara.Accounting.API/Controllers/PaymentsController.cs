using LinkPara.Accounting.Application.Features.Payments;
using LinkPara.Accounting.Application.Features.Payments.Commands.CancelPayment;
using LinkPara.Accounting.Application.Features.Payments.Commands.DeletePayment;
using LinkPara.Accounting.Application.Features.Payments.Commands.PostPayment;
using LinkPara.Accounting.Application.Features.Payments.Queries.GetFilterPayment;
using LinkPara.Accounting.Application.Features.Payments.Queries.GetPaymentById;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Accounting.API.Controllers;

public class PaymentsController : ApiControllerBase
{
    /// <summary>
    /// Returns filtered payment
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountingPayment:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<PaymentDto>>> GetFilterAsync([FromQuery] GetFilterPaymentQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a payment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountingPayment:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetPaymentByIdQuery { Id = id });
    }

    /// <summary>
    /// Post a payment
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountingPayment:Create")]
    [HttpPost("")]
    public async Task SavePaymentAsync(PostPaymentCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Cancel a payment
    /// </summary>
    /// <param name="clientReferenceId"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountingPayment:Delete")]
    [HttpDelete("{clientReferenceId}")]
    public async Task CancelPaymentAsync(Guid clientReferenceId)
    {
        await Mediator.Send(new CancelPaymentCommand { ClientReferenceId = clientReferenceId});
    }

    /// <summary>
    /// Delete a payment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountingPayment:Delete")]
    [HttpDelete("delete/{id}")]
    public async Task DeletePaymentAsync(Guid id)
    {
        await Mediator.Send(new DeletePaymentCommand { Id = id });
    }
}
