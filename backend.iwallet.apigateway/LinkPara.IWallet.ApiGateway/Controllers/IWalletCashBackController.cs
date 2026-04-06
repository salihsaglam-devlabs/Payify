using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;
using LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.IWallet.ApiGateway.Controllers;


public class IWalletCashBackController : ApiControllerBase
{
    private readonly ICashBackHttpClient _httpClient;

    public IWalletCashBackController(ICashBackHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// cashback.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize]
    public async Task<BaseServiceResponse<CashBackResponse>> CashBackAsync(CashBackRequest request)
    {
        return await _httpClient.CashBackAsync(request);
    }
}
