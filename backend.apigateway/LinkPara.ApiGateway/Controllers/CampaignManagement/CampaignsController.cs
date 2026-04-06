using LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.CampaignManagement;

public class CampaignsController : ApiControllerBase
{
    private readonly ICampaignHttpClient _campaignHttpClient;

    public CampaignsController(ICampaignHttpClient campaignHttpClient)
    {
        _campaignHttpClient = campaignHttpClient;
    }

    /// <summary>
    /// Get Campaigns
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy = "Campaign:ReadAll")]
    public async Task<ActionResult<List<CampaignResponse>>> GetCampaignsAsync()
    {
        return await _campaignHttpClient.GetCampaignsAsync();
    }
}
