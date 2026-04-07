using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Application.Features.Reconciliation.Commands.ApproveManualReview;
using LinkPara.Card.Application.Features.Reconciliation.Commands.ExecutePendingOperations;
using LinkPara.Card.Application.Features.Reconciliation.Commands.RejectManualReview;
using LinkPara.Card.Application.Features.Reconciliation.Commands.RegenerateOperations;
using LinkPara.Card.Application.Features.Reconciliation.Queries.GetPendingManualReviews;
using LinkPara.Card.Application.Features.Reconciliation.Queries.GetManualReviewDecisionPreview;
using LinkPara.Card.API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class ReconciliationController : ApiControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionRead)]
    [HttpGet(EndpointRoutes.ReconciliationManualReviewService.GetPendingManualReviews)]
    public async Task<IReadOnlyCollection<ReconciliationRunListItem>> GetPendingManualReviewsAsync(
        [FromQuery] GetPendingManualReviewsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await Mediator.Send(query, cancellationToken);
    }

    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionRead)]
    [HttpGet(EndpointRoutes.ReconciliationManualReviewService.GetManualReviewDecisionPreview)]
    public async Task<IActionResult> GetManualReviewDecisionPreviewAsync(
        [FromRoute] Guid manualReviewItemId,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetManualReviewDecisionPreviewQuery
        {
            ManualReviewItemId = manualReviewItemId
        }, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionCreate)]
    [HttpPost(EndpointRoutes.ReconciliationManualReviewService.ApproveManualReview)]
    public async Task<IActionResult> ApproveManualReviewAsync(
        [FromRoute] Guid manualReviewItemId,
        [FromBody] ManualReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new ApproveManualReviewCommand
        {
            ManualReviewItemId = manualReviewItemId,
            Note = request?.Note
        }, cancellationToken);

        return MapManualReviewResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionCreate)]
    [HttpPost(EndpointRoutes.ReconciliationManualReviewService.RejectManualReview)]
    public async Task<IActionResult> RejectManualReviewAsync(
        [FromRoute] Guid manualReviewItemId,
        [FromBody] ManualReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new RejectManualReviewCommand
        {
            ManualReviewItemId = manualReviewItemId,
            Note = request?.Note
        }, cancellationToken);

        return MapManualReviewResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionCreate)]
    [HttpPost(EndpointRoutes.ReconciliationAutoOperationService.ExecutePendingOperations)]
    public Task<ActionResult<ReconciliationExecutionSummary>> ExecutePendingOperationsAsync(
        [FromQuery] ExecutePendingOperationsCommand command,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithProcessLockAsync(
            lockName: ProcessLockNames.CardReconciliation,
            jobType: nameof(ExecutePendingOperationsCommand),
            action: token => Mediator.Send(command, token),
            cancellationToken: cancellationToken);
    }

    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionCreate)]
    [HttpPost(EndpointRoutes.ReconciliationService.RegenerateOperations)]
    public Task<ActionResult<ReconciliationProcessSummary>> RegenerateOperationsAsync(
        [FromQuery] RegenerateOperationsCommand command,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithProcessLockAsync(
            lockName: ProcessLockNames.CardReconciliation,
            jobType: nameof(RegenerateOperationsCommand),
            action: token => Mediator.Send(command, token),
            cancellationToken: cancellationToken);
    }

    private IActionResult MapManualReviewResult(ManualReviewOperationResult result)
    {
        return result.Status switch
        {
            ManualReviewOperationStatus.Success => Ok(result),
            ManualReviewOperationStatus.NotFound => NotFound(result),
            ManualReviewOperationStatus.AlreadyReviewed => Conflict(result),
            ManualReviewOperationStatus.StaleRun => Conflict(result),
            ManualReviewOperationStatus.OperationExecutionFailed => UnprocessableEntity(result),
            _ => BadRequest(result)
        };
    }
}
