using LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Requests;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace LinkPara.ApiGateway.Controllers.CampaignManagement;

public class IWalletCardsController : ApiControllerBase
{
    private readonly IIWalletCardHttpClient _httpClient;

    public IWalletCardsController(IIWalletCardHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get IWallet Card
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    [Authorize(Policy = "IWalletCard:Read")]
    public async Task<IWalletCardResponse> GetUserIWalletCardAsync()
    {
        return await _httpClient.GetUserIWalletCardAsync(new GetUserIWalletCardsFilterRequest());
    }

    /// <summary>
    /// Create Card
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "IWalletCard:Create")]
    public async Task<IWalletQrCodeResponse> CreateCardAsync([FromBody] IWalletCreateCardRequest request)
    {
        return await _httpClient.CreateCardAsync(request);
    }
}
