using LinkPara.Approval.Application.Features.Cases;
using LinkPara.Approval.Application.Features.Cases.Commands.DeleteCase;
using LinkPara.Approval.Application.Features.Cases.Commands.SaveCase;
using LinkPara.Approval.Application.Features.Cases.Commands.UpdateCase;
using LinkPara.Approval.Application.Features.Cases.Queries.GetById;
using LinkPara.Approval.Application.Features.Cases.Queries.GetCases;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Approval.API.Controllers;

public class CasesController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<PaginatedList<CaseDto>> GetAllCasesAsync([FromQuery] GetCasesQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "Case:Read")]
    [HttpGet("{id}")]
    public async Task<CaseDto> GetByIdAsync(Guid id)
    {
        return await Mediator.Send(new GetByIdQuery { Id = id});
    }

    [Authorize(Policy = "Case:Create")]
    [HttpPost("")]
    public async Task<Unit> SaveAsync(SaveCaseCommand command)
    {
        return await Mediator.Send(command);
    }

    [Authorize(Policy = "Case:Update")]
    [HttpPut("")]
    public async Task<Unit> UpdateAsync(UpdateCaseCommand command)
    {
        return await Mediator.Send(command);
    }

    [Authorize(Policy = "Case:Delete")]
    [HttpDelete("{id}")]
    public async Task<Unit> DeleteAsync(Guid id)
    {
        return await Mediator.Send(new DeleteCaseCommand { Id = id });
    }

}
