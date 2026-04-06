using LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Requests;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.CampaignManagement;

public class IWalletQrCodesController : ApiControllerBase
{
    private readonly IIWalletQrCodeHttpClient _httpClient;

    public IWalletQrCodesController(IIWalletQrCodeHttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    /// <summary>
    /// Create Card
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "IWalletQrCode:Create")]
    public async Task<IWalletQrCodeResponse> GenerateQrCodeAsync([FromBody] IWalletGenerateQrCodeRequest request)
    {
        return await _httpClient.GenerateQrCodeAsync(request);
    }
}