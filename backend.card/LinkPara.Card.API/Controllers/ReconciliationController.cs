using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Evaluate;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Execute;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Approve;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Reject;
using LinkPara.Card.Application.Features.Reconciliation.Queries.PendingReviews;
using LinkPara.Card.Application.Features.Reconciliation.Queries.GetAlerts;
using LinkPara.Card.Application.Features.Reconciliation.Commands;
using LinkPara.Card.Application.Features.Reconciliation.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers
{
    public class ReconciliationController : ApiControllerBase
    {
        //[Authorize(Policy = ReconciliationPolicies.Create)]
        [AllowAnonymous]
        [HttpPost("Evaluate")]
        public async Task<EvaluateResponse> Evaluate([FromBody] EvaluateRequest req = null, CancellationToken ct = default)
        {
            var cmd = new EvaluateCommand { Request = req ?? new EvaluateRequest() };

            var res = await Mediator.Send(cmd, ct);
            return res;
        }
        
        //[Authorize(Policy = ReconciliationPolicies.Create)]
        [AllowAnonymous]
        [HttpPost("Operations/Execute")]
        public async Task<ExecuteResponse> Execute([FromBody] ExecuteRequest req = null, CancellationToken ct = default)
        {
            var cmd = new ExecuteCommand { Request = req ?? new ExecuteRequest() };

            var res = await Mediator.Send(cmd, ct);
            return res;
        }

        //[Authorize(Policy = ReconciliationPolicies.Update)]
        [AllowAnonymous]
        [HttpPost("Reviews/Approve")]
        public async Task<ApproveResponse> Approve([FromBody] ApproveRequest req, CancellationToken ct = default)
        {
            var cmd = new ApproveCommand { Request = req ?? new ApproveRequest() };

            var res = await Mediator.Send(cmd, ct);
            return res;
        }

        //[Authorize(Policy = ReconciliationPolicies.Update)]
        [AllowAnonymous]
        [HttpPost("Reviews/Reject")]
        public async Task<RejectResponse> Reject([FromBody] RejectRequest req, CancellationToken ct = default)
        {
            var cmd = new RejectCommand { Request = req ?? new RejectRequest() };

            var res = await Mediator.Send(cmd, ct);
            return res;
        }

        //[Authorize(Policy = ReconciliationPolicies.ReadAll)]
        [AllowAnonymous]
        [HttpGet("Reviews/Pending")]
        public async Task<PendingReviewsResponse> GetPendingManualReviews([FromQuery] PendingReviewsRequest req, CancellationToken ct = default)
        {
            var q = new GetPendingReviewsQuery { Request = req ?? new PendingReviewsRequest() };
            var res = await Mediator.Send(q, ct);
            return res;
        }

        //[Authorize(Policy = ReconciliationPolicies.ReadAll)]
        [AllowAnonymous]
        [HttpGet("Alerts")]
        public async Task<GetAlertsResponse> GetAlerts([FromQuery] GetAlertsRequest req, CancellationToken ct = default)
        {
            var q = new GetAlertsQuery { Request = req ?? new GetAlertsRequest() };
            var res = await Mediator.Send(q, ct);
            return res;
        }
    }
}