using LinkPara.BusinessParameter.Application.Features.ParameterTemplates;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.DeleteParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.SaveParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.UpdateParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetAllParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetParameterTemplateById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.BusinessParameter.API.Controllers;

public class ParameterTemplatesController : ApiControllerBase
{
    /// <summary>
    /// Returns all parameter template by group code
    /// </summary>
    /// <param name="groupCode"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplate:ReadAll")]
    [HttpGet("{groupCode}")]
    public async Task<List<ParameterTemplateDto>> GetAllAsync([FromRoute] string groupCode)
    {
        return await Mediator.Send(new GetAllParameterTemplateByGroupCodeQuery { GroupCode = groupCode });
    }

    /// <summary>
    /// Returns all parameter template 
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplate:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<ParameterTemplateDto>>> GetAllAsync([FromQuery] GetAllParameterTemplateQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a parameter template
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplate:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveParameterTemplateCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Returns a parameter template
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplate:Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ParameterTemplateDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetParameterTemplateByIdQuery { Id = id });
    }

    /// <summary>
    /// Delete parameter template
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplate:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteParameterTemplateCommand { Id = id });
    }

    /// <summary>
    /// Updates a parameter template
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplate:Update")]
    [HttpPut("")]
    public async Task<ParameterTemplateDto> UpdateAsync(UpdateParameterTemplateCommand command)
    {
        return await Mediator.Send(command);
    }
}
