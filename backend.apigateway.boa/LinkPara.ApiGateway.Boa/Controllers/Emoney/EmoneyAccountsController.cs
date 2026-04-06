using LinkPara.ApiGateway.Boa.Filters.CustomerContext;
using LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Emoney;

public class EmoneyAccountsController : ApiControllerBase
{
    private readonly IEmoneyAccountHttpClient _httpClient;

    public EmoneyAccountsController(IEmoneyAccountHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Returns logged user's account   
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    [CustomerContextRequired]
    public async Task<AccountDto> GetAccountByUserIdAsync()
    {
        return await _httpClient.GetAccountByUserIdAsync(Guid.Parse(UserId));
    }

    /// <summary>
    /// Update status logged users's account.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [CustomerContextRequired]
    public async Task UpdateStatusAsync(UpdateAccountStatusRequest request)
    {
        var account = await _httpClient.GetAccountByUserIdAsync(Guid.Parse(UserId));

        if (account is not null)
        {
            var patchRequest = new JsonPatchDocument<UpdateAccountRequest>();
            patchRequest.Replace(s => s.ChangeReason, request.ChangeReason);
            patchRequest.Replace(s => s.AccountStatus, request.AccountStatus);

            await _httpClient.PatchAccountAsync(account.Id, patchRequest);
        }
    }
}
