using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class IncomingTransactionsController : ApiControllerBase
{
    private readonly IIncomingTransactionHttpClient _incomingTransactionHttpClient;

    public IncomingTransactionsController(
        IIncomingTransactionHttpClient incomingTransactionHttpClient)
    {
        _incomingTransactionHttpClient = incomingTransactionHttpClient;
    }

    /// <summary>
    /// Returns Incoming Transaction by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "IncomingTransaction:Read")]
    public async Task<ActionResult<IncomingTransactionDto>> GetIncomingTransactionByIdAsync(Guid id)
    {
        return await _incomingTransactionHttpClient.GetIncomingTransactionByIdAsync(id);
    }

    /// <summary>
    /// Returns Incoming Transactions by request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "IncomingTransaction:ReadAll")]
    public async Task<PaginatedList<IncomingTransactionDto>> GetIncomingTransactionsAsync([FromQuery] GetIncomingTransactionsRequest request)
    {
        return await _incomingTransactionHttpClient.GetIncomingTransactionsAsync(request);
    }

    /// <summary>
    /// Updates Transaction Status To ManualPayment.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "IncomingTransaction:Update")]
    public async Task ManualPaymentAsync(Guid id)
    {
        await _incomingTransactionHttpClient.ManualPaymentAsync(id);
    }
}
