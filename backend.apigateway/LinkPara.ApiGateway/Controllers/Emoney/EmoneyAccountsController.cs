using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.SharedModels.Pagination;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

public class EmoneyAccountsController : ApiControllerBase
{
    private readonly IEmoneyAccountHttpClient _accountHttpClient;

    public EmoneyAccountsController(IEmoneyAccountHttpClient accountHttpClient)
    {
        _accountHttpClient = accountHttpClient;
    }

    /// <summary>
    /// Returns logged user's account   
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    [Authorize(Policy = "EmoneyAccount:Read")]
    public async Task<AccountDto> GetAccountByUserIdAsync()
    {
        return await _accountHttpClient.GetAccountByUserIdAsync(Guid.Parse(UserId));
    }

    /// <summary>
    /// Validates user identity
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Update")]
    [HttpPost("validate-identity")]
    public async Task ValidateAccountUserIdentityAsync(ValidateIdentityRequest request)
    {
        await _accountHttpClient.ValidateAccountUserIdentityAsync(request, UserId);
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
    /// Updates an account
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Update")]
    [HttpPatch("{accountId}")]
    public async Task PatchAsync(Guid accountId, PatchAccountRequest request)
    {
        var patchRequest = new JsonPatchDocument<UpdateAccountRequest>();
        if (!string.IsNullOrEmpty(request.Profession))
        {
            patchRequest.Replace(s => s.Profession, request.Profession);
        }
        if (request.IsOpenBankingPermit.HasValue)
        {
            patchRequest.Replace(s => s.IsOpenBankingPermit, request.IsOpenBankingPermit);
        }
        await _accountHttpClient.PatchAccountAsync(accountId, patchRequest);
    }
}
