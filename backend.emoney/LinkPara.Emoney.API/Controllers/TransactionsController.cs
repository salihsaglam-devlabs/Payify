using LinkPara.Emoney.Application.Commons.Models.ReceiptModels;
using LinkPara.Emoney.Application.Features.Transactions;
using LinkPara.Emoney.Application.Features.Transactions.Queries.GetCustodyWalletTransactionsPartner;
using LinkPara.Emoney.Application.Features.Transactions.Queries.GetFastTransactionAmountsQuery;
using LinkPara.Emoney.Application.Features.Transactions.Queries.GetReceipt;
using LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionsByCustomerTransactionId;
using LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionDetails;
using LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionsAdmin;
using LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionSummary;
using LinkPara.Emoney.Application.Features.Transactions.Queries.GetWalletTransactions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class TransactionsController : ApiControllerBase
{
    /// <summary>
    /// Returns the transaction detail.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetTransactionDetailsAsync(Guid id)
        => await Mediator.Send(new GetTransactionDetailsQuery() { TransactionId = id });

    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("getByCustomerTransactionId/{customerTransactionId}")]
    public async Task<ActionResult<List<CustomerTransactionDto>>> GetTransactionsByCustomerTransactionIdAsync(string customerTransactionId)
        => await Mediator.Send(new GetTransactionsByCustomerTransactionIdQuery()
        {
            CustomerTransactionId = customerTransactionId
        });

    [Authorize(Policy = "Transaction:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<TransactionAdminDto>> GetAdminTransactionsAsync([FromQuery] GetTransactionsAdminQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("{userId}/summary")]
    public async Task<TransactionSummaryDto> GetTransactionSummary([FromRoute] Guid userId, [FromQuery] GetTransactionSummaryQuery query)
    {
        query.UserId = userId;
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns the transactions of a Custody Account wallet.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("getCustodyWalletTransactions")]
    public async Task<PaginatedList<TransactionDto>> GetCustodyWalletTransactionsAsync([FromQuery] GetCustodyWalletTransactionsQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("getCustodyWalletTransactionsPartner")]
    public async Task<PaginatedList<TransactionDto>> GetCustodyWalletTransactionsPartnerAsync([FromQuery] GetCustodyWalletTransactionsPartnerQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns most, last and user balance amount
    /// </summary>
    /// <param name="id"></param>
    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("user-fast-transaction-amounts/{id}")]
    public async Task<ActionResult<FastTransactionAmountsDto>> GetUserFastTransactionAmountsAsync(Guid id)
    => await Mediator.Send(new GetFastTransactionAmountsQuery() { WalletId = id });

    [Authorize(Policy = "Transaction:Read")]
    [HttpGet("{userId}/receipt")]
    public async Task<ReceiptResponse> GetReceipt([FromRoute] Guid userId, [FromQuery] GetReceiptQuery request)
    {
        request.UserId = userId;
        return await Mediator.Send(request);
    }
}
