using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.RepresentativeTransactions;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class RepresentativeTransactionsController : ApiControllerBase
{
    private readonly IRepresentativeTransactionHttpClient _client;

    public RepresentativeTransactionsController(IRepresentativeTransactionHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Get By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "RepresentativeTransactions:Read")]
    [HttpGet("{id}")]
    public async Task<RepresentativeTransactionDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await _client.GetByIdAsync(id);
    }

    /// <summary>
    /// Get Transactions List
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "RepresentativeTransactions:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<RepresentativeTransactionDto>> GetListAsync([FromQuery]GetRepresentativeTransactionsRequest request)
    {
        return await _client.GetListAsync(request);
    }

    /// <summary>
    /// Create New Transaction
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "RepresentativeTransactions:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveRepresentativeTransactionRequest request)
    {
        await _client.SaveAsync(request);
    }

    /// <summary>
    /// Update Representative Transaction
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "RepresentativeTransactions:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateRepresentativeTransactionRequest request)
    {
        await _client.UpdateAsync(request);
    }
}