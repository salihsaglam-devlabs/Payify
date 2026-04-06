using Microsoft.AspNetCore.Mvc;
using LinkPara.CampaignManagement.Application.Features.IWalletLocations;
using LinkPara.CampaignManagement.Application.Features.IWalletLocations.Queries.GetTowns;
using LinkPara.CampaignManagement.Application.Features.IWalletLocations.Queries.GetCities;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.CampaignManagement.API.Controllers;

public class IWalletLocationsController : ApiControllerBase
{
    /// <summary>
    /// Get Towns
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "IWalletLocation:ReadAll")]
    [HttpGet("cities/{cityId}/towns")]
    public async Task<List<TownDto>> GetTownsAsync([FromRoute]int cityId)
    {
        return await Mediator.Send(new GetTownsQuery { CityId = cityId});
    }

    /// <summary>
    /// Get Cities
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "IWalletLocation:ReadAll")]
    [HttpGet("cities")]
    public async Task<List<CityDto>> GetCitiesAsync()
    {
        return await Mediator.Send(new GetCitiesQuery());
    }

}
