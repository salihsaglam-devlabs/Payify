using LinkPara.ApiGateway.OpenBanking.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Requests;
using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.OpenBanking.Controllers;

//[Authorize(Policy = "YOS")]
public class DataController : ApiControllerBase
{
    private readonly IAccountServiceProviderHttpClient _httpClient;

    public DataController(IAccountServiceProviderHttpClient httpClient)
    {
        _httpClient = httpClient;
    }   

    /// <summary>
    /// This method used to get user identity information.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns user identity information</returns>
    [HttpGet("getIdentityInfo")]
    public async Task<ActionResult<IdentityInfoDto>> GetUserIdentityInfoAsync([FromQuery] GetUserIdentityInfoRequest request)
    {
        return await _httpClient.GetUserIdentityInfoAsync(request);
    }

    /// <summary>
    /// This method used to get user account list information.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns user account list</returns>
    [HttpGet("getAccountData")]
    public async Task<ActionResult<UserAccountResultDto>> GetUserAccountListAsync([FromQuery] GetUserAccountListRequest request)
    {
        var accountDetails =  await _httpClient.GetUserAccountListAsync(request);
        return new UserAccountResultDto
        {
            AccountDetailList = accountDetails
        };
    }
}
