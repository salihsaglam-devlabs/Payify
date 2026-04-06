using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.Transactions;
using LinkPara.PF.Application.Features.Transactions.Queries.GetAllTransactions;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.PF.API.Controllers;

public class TransactionsController : ApiControllerBase
{
    /// <summary>
    /// Returns all transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PfTransaction:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<TransactionDto>> GetAllAsync([FromQuery] GetAllTransactionQuery request)
    {
        return await Mediator.Send(request);
    }
}
