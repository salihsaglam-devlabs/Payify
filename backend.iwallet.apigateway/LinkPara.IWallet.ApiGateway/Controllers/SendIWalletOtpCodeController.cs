using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;
using LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace LinkPara.IWallet.ApiGateway.Controllers;

public class SendIWalletOtpCodeController : ApiControllerBase
{
    private readonly IOtpHttpClient _httpClient;

    public SendIWalletOtpCodeController(IOtpHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// SendOtpCode.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize]
    public async Task<BaseServiceResponse<SendIWalletSmsOtpResponse>> SendOtpCodeAsync(SendIWalletSmsOtpRequest request)
    {
        return await _httpClient.SendIWalletSmsOtpAsync(request);
    }
}
