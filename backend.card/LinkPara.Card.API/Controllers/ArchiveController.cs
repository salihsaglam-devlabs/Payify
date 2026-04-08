using LinkPara.Card.Application.Commons.Models.Archive;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Application.Features.Archive.Commands.RunArchive;
using LinkPara.Card.Application.Features.Archive.Queries.PreviewArchive;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers;

public class ArchiveController : ApiControllerBase
{
    [Authorize(Policy = ReconciliationPolicies.ReadAll)]
    [HttpPost("Preview")]
    public Task<ArchivePreviewResponse> PreviewAsync([FromBody] ArchivePreviewRequest? request, CancellationToken cancellationToken = default)
    {
        return Mediator.Send(new PreviewArchiveQuery { Request = request ?? new ArchivePreviewRequest() }, cancellationToken);
    }

    [Authorize(Policy = ReconciliationPolicies.Delete)]
    [HttpPost("Run")]
    public Task<ArchiveRunResponse> RunAsync([FromBody] ArchiveRunRequest? request, CancellationToken cancellationToken = default)
    {
        return Mediator.Send(new RunArchiveCommand { Request = request ?? new ArchiveRunRequest() }, cancellationToken);
    }
}
