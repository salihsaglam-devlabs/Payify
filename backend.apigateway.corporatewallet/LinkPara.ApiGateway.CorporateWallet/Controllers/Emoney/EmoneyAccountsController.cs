using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;

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
}
