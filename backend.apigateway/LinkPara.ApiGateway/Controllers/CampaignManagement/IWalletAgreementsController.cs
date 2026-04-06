using LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.CampaignManagement;

public class IWalletAgreementsController : ApiControllerBase
{
    private readonly IIWalletAgreementHttpClient _httpClient;

    public IWalletAgreementsController(IIWalletAgreementHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get iWallet Agreements
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "IWalletAgreement:ReadAll")]
    public async Task<List<IWalletAgreementResponse>> GetAgreementsAsync()
    {
        return await _httpClient.GetAgreementsAsync();
    }
}
