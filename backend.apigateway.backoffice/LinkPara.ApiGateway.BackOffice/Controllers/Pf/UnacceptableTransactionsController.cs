using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class UnacceptableTransactionsController : ApiControllerBase
{
    private readonly IUnacceptableTransactionHttpClient _unacceptableTransactionHttpClient;
    
    public UnacceptableTransactionsController(IUnacceptableTransactionHttpClient unacceptableTransactionHttpClient)
    {
        _unacceptableTransactionHttpClient = unacceptableTransactionHttpClient;
    }
    
    /// <summary>
    /// Retry unacceptable transaction
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosUnacceptable:Create")]
    [HttpPost("retry")]
    public async Task RetryUnacceptableAsync(RetryUnacceptableTransactionRequest request)
    {
        await _unacceptableTransactionHttpClient.RetryUnacceptableAsync(request);
    }
    
    /// <summary>
    /// Get all unacceptable records
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosUnacceptable:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<PhysicalPosUnacceptableTransactionDto>>> GetFilterAsync([FromQuery] GetAllUnacceptableTransactionRequest query)
    {
        return await _unacceptableTransactionHttpClient.GetAllUnacceptableTransactionsAsync(query);
    }
    
    /// <summary>
    /// Get unacceptable record with all related transactions
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosUnacceptable:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<UnacceptableTransactionDetailResponse>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _unacceptableTransactionHttpClient.GetDetailsByIdAsync(id);
    }

    /// <summary>
    /// Update unacceptable transaction current status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="unacceptableTransaction"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPosUnacceptable:Update")]
    [HttpPatch("{id}")]
    public async Task<PhysicalPosUnacceptableTransactionDto> Patch(Guid id, [FromBody] JsonPatchDocument<UpdateUnacceptableTransactionRequest> unacceptableTransaction)
    {
        return await _unacceptableTransactionHttpClient.UpdateStatusAsync(id, unacceptableTransaction);
    }
}