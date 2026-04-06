using LinkPara.CampaignManagement.Application.Features.Campaigns;
using LinkPara.CampaignManagement.Application.Features.Campaigns.Queries.GetCampaigns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.CampaignManagement.API.Controllers;

public class CampaignsController : ApiControllerBase
{
    /// <summary>
    /// Get Campaigns
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Campaign:ReadAll")]
    [HttpGet]
    public async Task<List<CampaignDto>> GetCampaignsAsync()
    {
        return await Mediator.Send(new GetCampaignsQuery());
    }
}
