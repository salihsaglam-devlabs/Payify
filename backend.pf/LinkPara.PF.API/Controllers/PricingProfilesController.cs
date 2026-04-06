using LinkPara.PF.Application.Commons.Models.PricingPreview;
using LinkPara.PF.Application.Commons.Models.PricingProfiles;
using LinkPara.PF.Application.Features.PricingProfiles;
using LinkPara.PF.Application.Features.PricingProfiles.Command.DeletePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Command.PatchPricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Command.SavePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePreviewPricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Queries.GetFilterPricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Queries.GetPricingProfileById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class PricingProfilesController : ApiControllerBase
{
    /// <summary>
    /// Returns a pricing profile with items
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PricingProfile:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PricingProfileDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetPricingProfileByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns filtered pricing profiles with items
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "PricingProfile:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<PricingProfileDto>>> GetFilterAsync([FromQuery] GetFilterPricingProfileQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new pricing profile with item
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PricingProfile:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SavePricingProfileCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates pricing profile with items
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PricingProfile:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdatePricingProfileCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete pricing profile
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "PricingProfile:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeletePricingProfileCommand { Id = id });
    }

    /// <summary>
    /// Updates pricing profile with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pricingProfile"></param>
    /// <returns></returns>
    [Authorize(Policy = "PricingProfile:Update")]
    [HttpPatch("{id}")]
    public async Task<UpdatePricingProfileRequest> Patch(Guid id, [FromBody] JsonPatchDocument<UpdatePricingProfileRequest> pricingProfile)
    {
        return await Mediator.Send(new PatchPricingProfileCommand { Id = id, PricingProfile = pricingProfile });
    }
    
    /// <summary>
    /// Checks loss-making rates in pricing profile update
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "PricingProfile:Update")]
    [HttpPost("update-preview")]
    public async Task<PricingProfilePreviewResponse> PreviewPricingProfileUpdateAsync(UpdatePreviewPricingProfileCommand command)
    {
        return await Mediator.Send(command);
    }
}
