using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class MoneyTransferTransactionsController : ApiControllerBase
{
    private readonly IMoneyTransferTransactionsHttpClient
        _moneyTransferTransactionsHttpClient;

    public MoneyTransferTransactionsController(
        IMoneyTransferTransactionsHttpClient moneyTransferTransactionHttpClient)
    {
        _moneyTransferTransactionsHttpClient = moneyTransferTransactionHttpClient;
    }

    [HttpGet("{Id}")]
    [Authorize(Policy = "MoneyTransferTransactions:Read")]
    public async Task<MoneyTransferTransactionsDto> GetTransactionByIdAsync(Guid Id)
    {
        return await _moneyTransferTransactionsHttpClient.GetTransactionByIdAsync(Id);
    }

    [HttpGet("")]
    [Authorize(Policy = "MoneyTransferTransactions:ReadAll")]
    public async Task<PaginatedList<MoneyTransferTransactionsDto>> GetTransactionsAsync(
        [FromQuery] GetMoneyTransferTransactionsRequest request)
    {
        return await _moneyTransferTransactionsHttpClient.GetTransactionsAsync(request);
    }

    /// <summary>
    /// Cancels Transaction.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("cancel")]
    [Authorize(Policy = "MoneyTransferTransactions:Update")]
    public async Task CancelAsync(Guid id)
    {
        await _moneyTransferTransactionsHttpClient.CancelAsync(id);
    }

    /// <summary>
    /// Updates Transaction Status To ManualPayment.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("manual-payment")]
    [Authorize(Policy = "MoneyTransferTransactions:Update")]
    public async Task CompleteAsync(Guid id)
    {
        await _moneyTransferTransactionsHttpClient.ManualPaymentAsync(id);
    }
}
