using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Commands.CreateTierPermission;
using LinkPara.Emoney.Application.Features.Limits.Commands.DisableTierPermission;
using LinkPara.Emoney.Application.Features.Limits.Commands.PatchTierPermission;
using LinkPara.Emoney.Application.Features.Limits.Commands.PutTierPermission;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierPermissionById;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierPermissionsQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class TierPermissionsController : ApiControllerBase
{
    /// <summary>
    /// Returns Tier permissions
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:ReadAll")]
    [HttpGet("")]
    public async Task<List<TierPermissionDto>> GetTierPermissionsAsync([FromQuery]GetTierPermissionsQuery request)
    {
        return await Mediator.Send(request);
    }
    
    /// <summary>
    /// Returns Tier permission by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Read")]
    [HttpGet("{id}")]
    public async Task<TierPermissionDto> GetTierPermissionByIdAsync(Guid id)
    {
        return await Mediator.Send(new GetTierPermissionByIdQuery { Id = id });
    }
      
    /// <summary>
    /// Creates a new tier permission.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Create")]
    [HttpPost("")]
    public async Task CreateTierPermissionAsync(CreateTierPermissionCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Partial updates tier permission.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Update")]
    [HttpPatch("{id}")]
    public async Task PatchTierPermissionAsync(Guid id, [FromBody] JsonPatchDocument<TierPermissionDto> command)
    {
        await Mediator.Send(new PatchTierPermissionCommand { Id = id, PatchTierPermission = command });
    }
    
    /// <summary>
    /// Updates tier permission.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Update")]
    [HttpPut("")]
    public async Task PutTierPermissionAsync(PutTierPermissionCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Disables tier permission.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Limit:Delete")]
    [HttpDelete("{id}")]
    public async Task DisableTierPermissionAsync(Guid id)
    {
        await Mediator.Send(new DisableTierPermissionCommand { Id = id });
    }
}