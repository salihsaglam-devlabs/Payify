using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class ReturnedTransactionsController : ApiControllerBase
{
    private readonly IReturnedTransactionHttpClient _returnedTransactionHttpClient;

    public ReturnedTransactionsController(
        IReturnedTransactionHttpClient returnedTransactionHttpClient)
    {
        _returnedTransactionHttpClient = returnedTransactionHttpClient;
    }

    /// <summary>
    /// Returns Returned Transaction by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "ReturnedTransaction:Read")]
    public async Task<ActionResult<ReturnedTransactionDto>> GetReturnedTransactionByIdAsync(Guid id)
    {
        return await _returnedTransactionHttpClient.GetReturnedTransactionByIdAsync(id);
    }

    /// <summary>
    /// Returns Returned Transactions by request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "ReturnedTransaction:ReadAll")]
    public async Task<PaginatedList<ReturnedTransactionDto>> GetReturnedTransactionsAsync([FromQuery] GetReturnedTransactionsRequest request)
    {
        return await _returnedTransactionHttpClient.GetReturnedTransactionsAsync(request);
    }

    /// <summary>
    /// Cancels Returned Transaction status.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ReturnedTransaction:Update")]
    public async Task CancelReturnedTransactionAsync(Guid id)
    {
        await _returnedTransactionHttpClient.CancelReturnedTransactionAsync(id);
    }
}
