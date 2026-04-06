using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class BankResponseCodesController : ApiControllerBase
{
    private readonly IBankResponseCodeHttpClient _client;

    public BankResponseCodesController(IBankResponseCodeHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Returns a response code.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "BankResponseCode:Read")]
    public async Task<BankResponseCodeDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await _client.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns all response codes.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "BankResponseCode:ReadAll")]
    public async Task<PaginatedList<BankResponseCodeDto>> GetListAsync([FromQuery] GetBankResponseCodeListRequest request)
    {
        return await _client.GetListAsync(request);
    }

    /// <summary>
    /// Saves bank bank response code.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "BankResponseCode:Create")]
    public async Task SaveAsync(SaveBankResponseCodeRequest request)
    {
        await _client.SaveAsync(request);
    }

    /// <summary>
    /// Updates a bank response code.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "BankResponseCode:Update")]
    public async Task UpdateAsync(UpdateBankResponseCodeRequest request)
    {
        await _client.UpdateAsync(request);
    }

    /// <summary>
    /// Deletes bank response code.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "BankResponseCode:Delete")]
    public async Task DeleteAsync(DeleteBankResponseCodeRequest request)
    {
        await _client.DeleteAsync(request);
    }
}
