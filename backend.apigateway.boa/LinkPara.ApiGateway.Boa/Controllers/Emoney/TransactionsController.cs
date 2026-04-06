using LinkPara.ApiGateway.Boa.Filters.CustomerContext;
using LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Emoney;

public class TransactionsController : ApiControllerBase
{
    private readonly ITransactionHttpClient _transactionHttpClient;

    public TransactionsController(ITransactionHttpClient transactionHttpClient)
    {
        _transactionHttpClient = transactionHttpClient;
    }

    /// <summary>
    /// Returns the transaction detail
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id}")]
    [CustomerContextRequired]
    public async Task<ActionResult<TransactionDto>> GetTransactionDetailsAsync(Guid id)
    {
        return await _transactionHttpClient.GetTransactionDetailsAsync(id);
    }

    /// <summary>
    /// Returns all transactions list of the wallet
    /// </summary>
    /// <param name="request"></param>
    [HttpGet("")]
    [CustomerContextRequired]
    public async Task<ActionResult<PaginatedList<TransactionDto>>> GetWalletTransactionsAsync(
        [FromQuery] GetWalletTransactionsRequest request)
    {
        return await _transactionHttpClient.GetWalletTransactionsAsync(request);
    }

    /// <summary>
    /// Returns general financial situation
    /// </summary>
    /// <param name="request"></param>
    [HttpGet("summary")]
    [CustomerContextRequired]
    public async Task<TransactionSummaryDto> GetTransactionSummary(
        [FromQuery] TransactionSummaryRequest request)
    {
        return await _transactionHttpClient.GetTransactionSummaryAsync(request);
    }

    [HttpGet("receipt")]
    [CustomerContextRequired]
    public async Task<ReceiptDto> GetReceipt([FromQuery] GetReceiptRequest request)
    {
        return await _transactionHttpClient.GetReceiptAsync(request);
    }
}
