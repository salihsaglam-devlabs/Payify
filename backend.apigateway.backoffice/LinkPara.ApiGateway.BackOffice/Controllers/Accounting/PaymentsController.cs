using LinkPara.ApiGateway.BackOffice.Services.Accounting.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Accounting;

public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentHttpClient _paymentHttpClient;

    public PaymentsController(IPaymentHttpClient paymentHttpClient)
    {
        _paymentHttpClient = paymentHttpClient;
    }

    /// <summary>
    /// Returns filtered Csutomers
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "AccountingPayment:ReadAll")]
    public async Task<ActionResult<PaginatedList<AccountingPaymentDto>>> GetFilterAsync([FromQuery] GetFilterPaymentRequest request)
    {
        return await _paymentHttpClient.GetListPaymentsAsync(request);
    }

    /// <summary>
    /// Returns a payment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "AccountingPayment:Read")]
    public async Task<ActionResult<AccountingPaymentDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _paymentHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Retry a payment
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "AccountingPayment:Create")]
    public async Task SavePaymentAsync(SavePaymentRequest request)
    {
        await _paymentHttpClient.SavePaymentAsync(request);
    }

    /// <summary>
    /// Delete a payment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("delete/{id}")]
    [Authorize(Policy = "AccountingPayment:Delete")]
    public async Task DeletePaymentAsync(Guid id)
    {
        await _paymentHttpClient.DeletePaymentAsync(id);
    }
}
