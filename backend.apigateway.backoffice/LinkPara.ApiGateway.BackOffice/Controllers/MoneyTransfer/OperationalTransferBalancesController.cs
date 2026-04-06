using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class OperationalTransferBalancesController : ApiControllerBase
{
    private readonly IOperationalTransferBalanceHttpClient _httpClient;

    public OperationalTransferBalancesController(IOperationalTransferBalanceHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Return operational transfer balance.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "OperationalTransferBalance:Read")]
    [HttpGet("{id}")]
    public async Task<OperationalTransferBalanceDto> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetOperationalTransferBalanceByIdAsync(id);
    }

    /// <summary>
    /// Returns operational transfer balances.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "OperationalTransferBalance:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<OperationalTransferBalanceDto>> GetListAsync([FromQuery] GetOperationalTransferBalanceListRequest request)
    {
        return await _httpClient.GetOperationalTransferBalanceListAsync(request);
    }

    /// <summary>
    /// Saves Operational Transfer Balance.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "OperationalTransferBalance:Create")]
    [HttpPost("")]
    public async Task SaveAsync([FromBody] SaveOperationalTransferBalanceRequest request)
    {
        await _httpClient.SaveOperationalTransferBalanceAsync(request);
    }


    /// <summary>
    /// Partial updates customized Operational Transfer Balance.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "OperationalTransferBalance:Update")]
    [HttpPatch("{id}")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<PatchOperationalTransferBalanceRequest> request)
    {
        await _httpClient.PatchOperationalTransferBalanceAsync(id, request);
    }


}
