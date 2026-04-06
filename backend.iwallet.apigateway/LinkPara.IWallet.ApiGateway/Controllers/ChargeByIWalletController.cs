using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;
using LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace LinkPara.IWallet.ApiGateway.Controllers;

public class ChargeByIWalletController : ApiControllerBase
{
    private readonly IChargeHttpClient _httpClient;

    public ChargeByIWalletController(IChargeHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Charge.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize]
    public async Task<BaseServiceResponse<ChargeResponse>> ChargeByIWalletAsync(ChargeRequest request)
    {
        return await _httpClient.ChargeAsync(request);
    }
}
