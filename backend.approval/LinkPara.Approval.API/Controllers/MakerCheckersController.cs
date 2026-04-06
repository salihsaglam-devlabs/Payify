using LinkPara.Approval.Application.Features.MakerCheckers;
using LinkPara.Approval.Application.Features.MakerCheckers.Commands.DeleteMakerChecker;
using LinkPara.Approval.Application.Features.MakerCheckers.Commands.SaveMakerChecker;
using LinkPara.Approval.Application.Features.MakerCheckers.Commands.UpdateMakerChecker;
using LinkPara.Approval.Application.Features.MakerCheckers.Queries.GetMakerChecker;
using LinkPara.Approval.Application.Features.MakerCheckers.Queries.GetMakerCheckersByCaseId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Approval.API.Controllers;

public class MakerCheckersController : ApiControllerBase
{
    [Authorize(Policy = "MakerChecker:Read")]
    [HttpGet("")]
    public async Task<MakerCheckerDto> GetByIdAsync(Guid id)
    {
        return await Mediator.Send(new GetMakerCheckerQuery { Id = id });
    }

    [Authorize(Policy = "MakerChecker:ReadAll")]
    [HttpGet("case-makercheckers")]
    public async Task<List<MakerCheckerDto>> GetByCaseIdAsync(Guid caseId)
    {
        return await Mediator.Send(new GetMakerCheckersByCaseIdQuery { CaseId = caseId });
    }

    [Authorize(Policy = "MakerChecker:Create")]
    [HttpPost("")]
    public async Task<Unit> SaveAsync(SaveMakerCheckerCommand command)
    {
        return await Mediator.Send(command);
    }

    [Authorize(Policy = "MakerChecker:Update")]
    [HttpPut("")]
    public async Task<Unit> UpdateAsync(UpdateMakerCheckerCommand command)
    {
        return await Mediator.Send(command);
    }

    [Authorize(Policy = "MakerChecker:Delete")]
    [HttpDelete("{id}")]
    public async Task<Unit> DeleteAsync(Guid id)
    {
        return await Mediator.Send(new DeleteMakerCheckerCommand { Id = id });
    }
}
