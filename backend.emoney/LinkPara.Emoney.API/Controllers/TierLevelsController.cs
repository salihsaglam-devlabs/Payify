using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Commands.CheckOrUpgradeAccountTier;
using LinkPara.Emoney.Application.Features.Limits.Commands.CreateAccountCustomTierLevel;
using LinkPara.Emoney.Application.Features.Limits.Commands.CreateCustomTierLevel;
using LinkPara.Emoney.Application.Features.Limits.Commands.DeleteAccountCustomTier;
using LinkPara.Emoney.Application.Features.Limits.Commands.DisableCustomTierLevel;
using LinkPara.Emoney.Application.Features.Limits.Commands.PatchCustomTierLevel;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierLevelById;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierLevels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class TierLevelsController : ApiControllerBase
{
    /// <summary>
    /// Returns Tier levels
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:ReadAll")]
    [HttpGet("")]
    public async Task<List<TierLevelDto>> GetTierLevelsAsync([FromQuery]GetTierLevelsQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns Tier level by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Read")]
    [HttpGet("{id}")]
    public async Task<TierLevelDto> GetTierLevelByIdAsync(Guid id)
    {
        return await Mediator.Send(new GetTierLevelByIdQuery { Id = id });
    }
    
    /// <summary>
    /// Creates a new custom limit level.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Create")]
    [HttpPost("")]
    public async Task CreateCustomTierLevelAsync(CustomTierLevelDto command)
    {
        await Mediator.Send(new CreateCustomTierLevelCommand { TierLevelDto = command });
    }

    /// <summary>
    /// Partial updates customized tier level.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Update")]
    [HttpPatch("{id}")]
    public async Task PatchCustomTierLevelAsync(Guid id, [FromBody] JsonPatchDocument<CustomTierLevelDto> command)
    {
        await Mediator.Send(new PatchCustomTierLevelCommand { Id = id, PatchCustomTierLevel = command });
    }

    /// <summary>
    /// Disables custom tier level.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Delete")]
    [HttpDelete("{id}")]
    public async Task DisableCustomTierLevelAsync(Guid id)
    {
        await Mediator.Send(new DisableCustomTierLevelCommand { Id = id });
    }


    /// <summary>
    /// Creates a new account custom limit level.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Create")]
    [HttpPost("account-custom-tier")]
    public async Task CreateAccountCustomTierLevelAsync(AccountCustomTierDto command)
    {
        await Mediator.Send(new CreateAccountCustomTierLevelCommand { AccountCustomTierDto = command });
    }

    [Authorize(Policy = "Limit:Delete")]
    [HttpDelete("account-custom-tier/{id}")]
    public async Task DeleteAccountCustomTierAsync(Guid id)
    {
        await Mediator.Send(new DeleteAccountCustomTierCommand { Id = id });
    }
    
    /// <summary>
    /// Checks whether the account needs to be upgraded. Increases to required level when needed.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Update")]
    [HttpPost("upgrade-account-tier")]
    public async Task CheckOrUpgradeAccountTierAsync(CheckOrUpgradeAccountTierCommand command)
    {
        await Mediator.Send(command);
    }
}