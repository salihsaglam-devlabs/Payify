using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Evaluate;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Execute;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Approve;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Reject;
using LinkPara.Card.Application.Features.Reconciliation.Queries.PendingReviews;
using LinkPara.Card.Application.Features.Reconciliation.Queries.GetAlerts;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers
{
    public class ReconciliationController : ApiControllerBase
    {
        [Authorize(Policy = ReconciliationPolicies.Create)]
        [HttpPost("Evaluate")]
        public async Task<EvaluateResponse> Evaluate([FromBody] EvaluateRequest req = null, CancellationToken ct = default)
        {
            var cmd = new EvaluateCommand { Request = req ?? new EvaluateRequest() };

            var res = await Mediator.Send(cmd, ct);
            return res;
        }
        
        [Authorize(Policy = ReconciliationPolicies.Create)]
        [HttpPost("Operations/Execute")]
        public async Task<ExecuteResponse> Execute([FromBody] ExecuteRequest req = null, CancellationToken ct = default)
        {
            var cmd = new ExecuteCommand { Request = req ?? new ExecuteRequest() };

            var res = await Mediator.Send(cmd, ct);
            return res;
        }

        [Authorize(Policy = ReconciliationPolicies.Update)]
        [HttpPost("Reviews/Approve")]
        public async Task<ApproveResponse> Approve([FromBody] ApproveRequest req, CancellationToken ct = default)
        {
            var cmd = new ApproveCommand { Request = req ?? new ApproveRequest() };

            var res = await Mediator.Send(cmd, ct);
            return res;
        }

        [Authorize(Policy = ReconciliationPolicies.Update)]
        [HttpPost("Reviews/Reject")]
        public async Task<RejectResponse> Reject([FromBody] RejectRequest req, CancellationToken ct = default)
        {
            var cmd = new RejectCommand { Request = req ?? new RejectRequest() };

            var res = await Mediator.Send(cmd, ct);
            return res;
        }

        [Authorize(Policy = ReconciliationPolicies.ReadAll)]
        [HttpGet("Reviews/Pending")]
        public async Task<PaginatedList<ManualReview>> GetPendingManualReviews([FromQuery] GetPendingReviewsQuery query, CancellationToken ct = default)
        {
            return await Mediator.Send(query, ct);
        }

        [Authorize(Policy = ReconciliationPolicies.ReadAll)]
        [HttpGet("Alerts")]
        public async Task<PaginatedList<Alert>> GetAlerts([FromQuery] GetAlertsQuery query, CancellationToken ct = default)
        {
            return await Mediator.Send(query, ct);
        }
    }
}