using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Queries.GetAllParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups;
using Microsoft.AspNetCore.Mvc;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.SaveParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Queries.GetParameterGroupById;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.DeleteParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.UpdateParameterGroup;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.BusinessParameter.API.Controllers;

public class ParameterGroupsController : ApiControllerBase
{
    /// <summary>
    /// Creates a parameterGroup that make an application
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterGroup:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveParameterGroupCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Returns all parameter group
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "ParameterGroup:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<ParameterGroupDto>>> GetAllAsync([FromQuery] GetAllParameterGroupQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a parameter group that make an application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterGroup:Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ParameterGroupDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetParameterGroupByIdQuery { Id = id });
    }

    /// <summary>
    /// Delete parameter group
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterGroup:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteParameterGroupCommand { Id = id });
    }

    /// <summary>
    /// Updates a parameter group
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterGroup:Update")]
    [HttpPut("")]
    public async Task<ActionResult<ParameterGroupDto>> UpdateAsync(UpdateParameterGroupCommand command)
    {
        return await Mediator.Send(command);
    }

}
