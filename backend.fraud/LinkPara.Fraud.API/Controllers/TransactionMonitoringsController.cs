using LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Response;
using LinkPara.Fraud.Application.Features.Transactions;
using LinkPara.Fraud.Application.Features.Transactions.Commands.CancelTransactions;
using LinkPara.Fraud.Application.Features.Transactions.Commands.ExecuteTransactions;
using LinkPara.Fraud.Application.Features.Transactions.Commands.ResumeTransactions;
using LinkPara.Fraud.Application.Features.Transactions.Commands.TransactionDetails;
using LinkPara.Fraud.Application.Features.Transactions.Queries.GetAllTransactions;
using LinkPara.Fraud.Application.Features.Transactions.Queries.GetTransactions;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static LinkPara.Fraud.Application.Features.Transactions.Commands.ResumeTransactions.ResumeTransactionCommandHandler;

namespace LinkPara.Fraud.API.Controllers;

public class TransactionMonitoringsController : ApiControllerBase
{
    /// <summary>
    /// Returns transaction monitoring logs
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "FraudTransactionMonitorings:ReadAll")]
    public async Task<PaginatedList<TransactionMonitoringDto>> GetAllAsync([FromQuery] GetAllTransactionQuery query)
    {
        return await Mediator.Send(query);
    }
    /// <summary>
    /// Returns a transaction
    /// </summary>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    [Authorize(Policy = "FraudTransaction:Read")]
    [HttpGet("{transactionId}")]
    public async Task<ActionResult<TransactionApiResponse>> GetTransactionAsync([FromRoute] string transactionId)
    {
        return await Mediator.Send(new GetTransactionQuery { TransactionId = transactionId });
    }

    /// <summary>
    /// Create transaction
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "FraudTransaction:Create")]
    [HttpPost("")]
    public async Task<ActionResult<TransactionResponse>> ExecuteTransactionAsync(FraudCheckRequest command)
    {
        return await Mediator.Send(new ExecuteTransactionCommand { FraudCheckRequest = command});
    }

    /// <summary>
    /// Returns a transaction details
    /// </summary>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    [Authorize(Policy = "FraudTransaction:Read")]
    [HttpPost("detail/{transactionId}")]
    public async Task<ActionResult<TransactionDetailResponse>> GetTransactionDetailAsync([FromRoute] string transactionId)
    {
        return await Mediator.Send(new TransactionDetailCommand { TransactionId = transactionId });
    }

    /// <summary>
    /// Cancel transaction
    /// </summary>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    [Authorize(Policy = "FraudTransaction:Update")]
    [HttpPost("cancel/{transactionId}")]
    public async Task<ActionResult<CancelTransactionResponse>> CancelTransactionAsync([FromRoute] string transactionId)
    {
        return await Mediator.Send(new CancelTransactionCommand { TransactionId = transactionId });
    }

    /// <summary>
    /// Resume transaction
    /// </summary>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    [Authorize(Policy = "FraudTransaction:Update")]
    [HttpPost("resume/{transactionId}")]
    public async Task<ResumeRequest> ResumeAsync([FromRoute] Guid transactionId)
    {
        return await Mediator.Send(new ResumeTransactionCommand { TransactionId = transactionId });
    }
}
