using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.BranchTransactions;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class BranchTransactionsController : ApiControllerBase
{
    private readonly IBranchTransactionHttpClient _client;

    public BranchTransactionsController(IBranchTransactionHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Get By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "BranchTransactions:Read")]
    [HttpGet("{id}")]
    public async Task<BranchTransactionDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await _client.GetByIdAsync(id);
    }

    /// <summary>
    /// Get Transactions List
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BranchTransactions:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<BranchTransactionDto>> GetListAsync([FromQuery] GetBranchTransactionsRequest request)
    {
        return await _client.GetListAsync(request);
    }

    /// <summary>
    /// Create New Transaction
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BranchTransactions:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveBranchTransactionRequest request)
    {
        await _client.SaveAsync(request);
    }

    /// <summary>
    /// Update Branch Transaction
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BranchTransactions:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateBranchTransactionRequest request)
    {
        await _client.UpdateAsync(request);
    }
}