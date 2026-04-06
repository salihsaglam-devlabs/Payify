using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.CostProfiles.Queries.GetCostProfileById;
using LinkPara.PF.Application.Features.CostProfiles;
using LinkPara.SharedModels.Pagination;
using LinkPara.PF.Application.Features.CostProfiles.Queries.GetFilterCostProfile;
using LinkPara.PF.Application.Features.CostProfiles.Command.SaveCostProfile;
using Microsoft.AspNetCore.JsonPatch;
using LinkPara.PF.Application.Commons.Models.CostProfiles;
using LinkPara.PF.Application.Commons.Models.PricingPreview;
using LinkPara.PF.Application.Features.CostProfiles.Command.PatchCostProfile;
using LinkPara.PF.Application.Features.CostProfiles.Command.UpdatePreviewCostProfile;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.PF.API.Controllers;

public class CostProfilesController : ApiControllerBase
{
    /// <summary>
    /// Returns a cost profile with items 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "CostProfile:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<CostProfilesDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetCostProfileByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns filtered cost profiles with items
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "CostProfile:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<CostProfilesDto>>> GetFilterAsync([FromQuery] GetFilterCostProfileQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new cost profile with item
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "CostProfile:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveCostProfileCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates cost profile with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="costProfile"></param>
    /// <returns></returns>
    [Authorize(Policy = "CostProfile:Update")]
    [HttpPatch("{id}")]
    public async Task<UpdateCostProfileRequest> PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateCostProfileRequest> costProfile)
    {
        return await Mediator.Send(new PatchCostProfileCommand { Id = id, CostProfile = costProfile });
    }
    
    /// <summary>
    /// Checks loss-making rates in cost profile update
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "CostProfile:Update")]
    [HttpPost("update-preview")]
    public async Task<CostProfilePreviewResponse> PreviewCostProfileUpdateAsync(UpdatePreviewCostProfileCommand command)
    {
        return await Mediator.Send(command);
    }
}
