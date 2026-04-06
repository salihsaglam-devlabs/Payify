using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;
using LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.IWallet.ApiGateway.Controllers;

public class LoginController : ApiControllerBase
{
    private readonly ILoginHttpClient _httpClient;

    public LoginController(ILoginHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Login.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task<BaseServiceResponse<LoginResponse>> ProvisionAsync(LoginRequest request)
    {
        return await _httpClient.LoginAsync(request);
    }
}
