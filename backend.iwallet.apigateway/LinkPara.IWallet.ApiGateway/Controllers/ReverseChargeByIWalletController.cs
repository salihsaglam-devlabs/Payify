using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;
using LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.IWallet.ApiGateway.Controllers;

public class ReverseChargeByIWalletController : ApiControllerBase
{
    private readonly IReverseChargeHttpClient _httpClient;

    public ReverseChargeByIWalletController(IReverseChargeHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// reverse charge.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize]
    public async Task<BaseServiceResponse<ReverseChargeResponse>> CashBackAsync(ReverseChargeRequest request)
    {
        return await _httpClient.ReverseChargeAsync(request);
    }

}
