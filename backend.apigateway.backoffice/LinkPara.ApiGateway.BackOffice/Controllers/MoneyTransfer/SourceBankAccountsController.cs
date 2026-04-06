using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class SourceBankAccountsController : ApiControllerBase
{
    private readonly ISourceBankAccountHttpClient _client;

    public SourceBankAccountsController(ISourceBankAccountHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Returns bank account of company.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "SourceBankAccount:Read")]
    public async Task<SourceBankAccountDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await _client.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns all accounts.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "SourceBankAccount:ReadAll")]
    public async Task<PaginatedList<SourceBankAccountDto>> GetListAsync([FromQuery] GetSourceBankAccountListRequest request)
    {
        return await _client.GetListAsync(request);
    }

    /// <summary>
    /// Returns all account banks.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SourceBankAccount:ReadAll")]
    [HttpGet("account-banks")]
    public async Task<List<BankModel>> GetAccountBanksAsync([FromQuery] GetAccountBanksRequest request)
    {
        return await _client.GetAccountBanksAsync(request);
    }

    /// <summary>
    /// Saves bank account for company.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "SourceBankAccount:Create")]
    public async Task SaveAsync(SaveSourceBankAccountRequest request)
    {
        await _client.SaveAsync(request);
    }

    /// <summary>
    /// Updates bank account for company.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "SourceBankAccount:Update")]
    public async Task UpdateAsync(UpdateSourceBankAccountRequest request)
    {
        await _client.UpdateAsync(request);
    }

    /// <summary>
    /// Deletes bank account for company.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "SourceBankAccount:Delete")]
    public async Task DeleteAsync(DeleteSourceBankAccountRequest request)
    {
        await _client.DeleteAsync(request);
    }
}
