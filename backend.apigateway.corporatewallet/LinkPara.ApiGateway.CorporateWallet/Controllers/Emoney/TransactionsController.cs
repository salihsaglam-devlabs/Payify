using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;

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
    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetTransactionDetailsAsync(Guid id)
    {
        return await _transactionHttpClient.GetTransactionDetailsAsync(id);
    }

    /// <summary>
    /// Returns all transactions list of the wallet
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "Transaction:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<TransactionDto>>> GetWalletTransactionsAsync(
        [FromQuery] GetWalletTransactionsRequest request)
    {
        return await _transactionHttpClient.GetWalletTransactionsAsync(request);
    }

    /// <summary>
    /// Returns general financial situation
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("summary")]
    public async Task<TransactionSummaryDto> GetTransactionSummary(
        [FromQuery] TransactionSummaryRequest request)
    {
        return await _transactionHttpClient.GetTransactionSummaryAsync(request);
    }

}