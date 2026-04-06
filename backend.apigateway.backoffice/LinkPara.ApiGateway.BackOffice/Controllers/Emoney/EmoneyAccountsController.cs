using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class EmoneyAccountsController : ApiControllerBase
{
    private readonly IEmoneyAccountHttpClient _accountHttpClient;

    public EmoneyAccountsController(IEmoneyAccountHttpClient accountHttpClient)
    {
        _accountHttpClient = accountHttpClient;
    }

    /// <summary>
    /// Returns an account
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "EmoneyAccount:Read")]
    public async Task<AccountDto> GetAccountByIdAsync([FromRoute] Guid id)
    {
        return await _accountHttpClient.GetAccountByIdAsync(id);
    }

    /// <summary>
    /// Returns the all accounts.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "EmoneyAccount:ReadAll")]
    public async Task<PaginatedList<AccountDto>> GetAccountList([FromQuery] GetAccountListRequest request)
    {
        return await _accountHttpClient.GetAccountListAsync(request);
    }

    /// <summary>
    /// Returns users of an account
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [HttpGet("{accountId}/users")]
    [Authorize(Policy = "EmoneyAccount:Read")]
    public async Task<List<AccountUserDto>> GetAccountUserList([FromRoute] Guid accountId)
    {
        return await _accountHttpClient.GetAccountUserListAsync(accountId);
    }

    /// <summary>
    /// returns wallets of an account.
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [HttpGet("{accountId}/wallets")]
    [Authorize(Policy = "EmoneyAccount:Read")]
    public async Task<List<WalletDto>> GetAccountWalletList([FromRoute] Guid accountId)
    {
        return await _accountHttpClient.GetAccountWalletListAsync(accountId);
    }

    /// <summary>
    /// Returns all account user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("accountUsers")]
    [Authorize(Policy = "EmoneyAccount:ReadAll")]
    public async Task<ActionResult<PaginatedList<AccountUserDto>>> GetAllAsync([FromQuery] GetAllAccountUserRequest request)
    {
        return await _accountHttpClient.GetAllAccountUserAsync(request);
    }

    /// <summary>
    /// Updates an account
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{accountId}")]
    [Authorize(Policy = "EmoneyAccount:Update")]
    public async Task<AccountDto> PatchAsync(Guid accountId, [FromBody] JsonPatchDocument<UpdateAccountRequest> request)
    {
        return await _accountHttpClient.PatchAsync(accountId, request);
    }
    
    /// <summary>
    /// Returns account kyc changes
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Read")]
    [HttpGet("{id}/kyc-changes")]
    public async Task<List<AccountKycChangeDto>> GetAccountKycChangesByIdAsync([FromRoute] Guid id)
    {
        return await _accountHttpClient.GetAccountKycChangesByIdAsync(id);
    }

    /// <summary>
    /// Returns CustodyAccounts according to filter
    /// </summary>
    /// <param name="query">Filter</param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Read")]
    [HttpGet("getCustodyAccountList/")]
    public async Task<PaginatedList<CustodyAccountResponse>> GetCustodyAccountListAsync([FromQuery] GetCustodyAccountListRequest query)
    {
        return await _accountHttpClient.GetCustodyAccountListAsync(query);
    }

    /// <summary>
    /// Deactivates Account
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>    
    [Authorize(Policy = "EmoneyAccount:Update")]
    [HttpPost("deactivate-account")]
    public async Task DeactivateAccountAsync(DeactivateAccountRequest request)
    {
        await _accountHttpClient.DeactivateAccountAsync(request);
    }
}
