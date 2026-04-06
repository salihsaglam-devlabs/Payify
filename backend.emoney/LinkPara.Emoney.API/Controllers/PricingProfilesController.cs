using LinkPara.Emoney.Application.Features.PricingProfiles;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.DeletePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.SavePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfileItem;
using LinkPara.Emoney.Application.Features.PricingProfiles.Queries.GetCardTopupCommission;
using LinkPara.Emoney.Application.Features.PricingProfiles.Queries.GetPricingProfileById;
using LinkPara.Emoney.Application.Features.PricingProfiles.Queries.GetPricingProfileList;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class PricingProfilesController : ApiControllerBase
{
    /// <summary>
    /// Returns a profile.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyPricingProfiles:Read")]
    [HttpGet("{id}")]
    public async Task<PricingProfileDto> GetByIdAsync(Guid id)
    {
        return await Mediator.Send(new GetPricingProfileByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns all profiles.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyPricingProfiles:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<PricingProfileDto>> GetListAsync([FromQuery] GetPricingProfileListQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns commission rate of active profile item.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyPricingProfiles:Read")]
    [HttpGet("card-commission")]
    public async Task<PricingProfileItemDto> GetCardTopupCommissionAsync()
    {
        return await Mediator.Send(new GetCardTopupCommissionQuery());
    }

    /// <summary>
    /// Creates a new profile with items
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyPricingProfiles:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SavePricingProfileCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates profile and its items.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyPricingProfiles:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdatePricingProfileCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates a profile item.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyPricingProfiles:Update")]
    [HttpPut("update-item")]
    public async Task UpdateItemAsync(UpdatePricingProfileItemCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Deletes profile's given item.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyPricingProfiles:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeletePricingProfileItemCommand { Id = id});
    }
}
