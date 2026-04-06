using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.DeleteParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.SaveParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.UpdateParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetAllParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetParameterTemplateValueById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.BusinessParameter.API.Controllers;

public class ParameterTemplateValuesController : ApiControllerBase
{
    /// <summary>
    /// Returns all parameter template value by group code and parameter code
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplateValue:ReadAll")]
    [HttpGet("getAll")]
    public async Task<List<ParameterTemplateValueDto>> GetAllAsync([FromQuery] GetAllParameterTemplateValueByGroupCodeQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns all parameter template value
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplateValue:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<ParameterTemplateValueDto>>> GetAllAsync([FromQuery]GetAllParameterTemplateValueQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a parameter template value
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplateValue:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveParameterTemplateValueCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Returns a parameter template value
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplateValue:Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ParameterTemplateValueDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetParameterTemplateValueByIdQuery { Id = id });
    }

    /// <summary>
    /// Delete parameter template value
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplateValue:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteParameterTemplateValueCommand { Id = id });
    }

    /// <summary>
    /// Updates a parameter template value
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "ParameterTemplateValue:Update")]
    [HttpPut("")]
    public async Task<ParameterTemplateValueDto> UpdateAsync(UpdateParameterTemplateValueCommand command)
    {
        return await Mediator.Send(command);
    }
}
