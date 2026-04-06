using LinkPara.PF.Application.Features.PricingProfiles;
using LinkPara.PF.Application.Features.PricingProfiles.Queries.GetFilterPricingProfile;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers.Boa;

public class BoaPricingProfilesController : ApiControllerBase
{
    /// <summary>
    /// Returns filtered pricing profiles with items
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<PricingProfileDto>>> GetFilterAsync([FromQuery] GetFilterPricingProfileQuery query)
    {
        return await Mediator.Send(query);
    }
}