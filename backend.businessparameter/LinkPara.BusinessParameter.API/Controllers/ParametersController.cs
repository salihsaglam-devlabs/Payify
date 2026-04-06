using LinkPara.BusinessParameter.Application.Features.Parameters;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.DeleteParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.SaveParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.UpdateParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetAllParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetCompanyInformationParameters;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetParameterById;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetProfessionParameters;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.BusinessParameter.API.Controllers;

public class ParametersController : ApiControllerBase
{
    /// <summary>
    /// Returns all parameter by group code
    /// </summary>
    /// <param name="groupCode"></param>
    /// <returns></returns>
    [Authorize(Policy = "Parameter:ReadAll")]
    [HttpGet("{groupCode}")]
    public async Task<List<ParameterDto>> GetAllAsync([FromRoute] string groupCode)
    {
        return await Mediator.Send(new GetAllParameterByGroupCodeQuery { GroupCode = groupCode });
    }

    /// <summary>
    /// Returns a parameter
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Parameter:Read")]
    [HttpGet("getByCode")]
    public async Task<ParameterDto> GetByCodeAsync([FromQuery] GetParameterQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a parameter
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Parameter:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveParameterCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Returns all parameter
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Parameter:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<ParameterDto>>> GetAllAsync([FromQuery] GetAllParameterQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "Parameter:Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ParameterDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetParameterByIdQuery { Id = id });
    }

    /// <summary>
    /// Delete parameter
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Parameter:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteParameterCommand { Id = id });
    }


    /// <summary>
    /// Updates a parameter template
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Parameter:Update")]
    [HttpPut("")]
    public async Task<ParameterDto> UpdateAsync(UpdateParameterCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Returns profession parameters
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("professions")]
    public async Task<List<ParameterDto>> GetProfessionParametersAsync()
    {
        return await Mediator.Send(new GetProfessionParametersAsyncQuery { });
    }

    /// <summary>
    /// Returns company information parameters
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("company-info")]
    public async Task<List<ParameterDto>> GetCompanyInfoParametersAsync()
    {
        return await Mediator.Send(new GetCompanyInformationParametersAsyncQuery { });
    }
}
