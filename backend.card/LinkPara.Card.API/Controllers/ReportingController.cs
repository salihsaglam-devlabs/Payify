using LinkPara.Card.Application.Commons.Models.Reporting;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Application.Features.Reporting.Queries.GetProblemRecords;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryByFile;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryByNetwork;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryDaily;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryOverall;
using LinkPara.Card.Application.Features.Reporting.Queries.GetTransactions;
using LinkPara.Card.Application.Features.Reporting.Queries.GetUnmatchedRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers;

[Route("v1/Reporting/Reconciliation")]
public class ReportingController : ApiControllerBase
{
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Transactions")]
    public async Task<GetTransactionsResponse> GetTransactions(
        [FromQuery] GetTransactionsQuery query, CancellationToken ct = default)
    {
        return await Mediator.Send(query, ct);
    }
    
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Problems")]
    public async Task<GetTransactionsResponse> GetProblemRecords(
        [FromQuery] GetProblemRecordsQuery query, CancellationToken ct = default)
    {
        return await Mediator.Send(query, ct);
    }
    
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Unmatched")]
    public async Task<GetTransactionsResponse> GetUnmatchedRecords(
        [FromQuery] GetUnmatchedRecordsQuery query, CancellationToken ct = default)
    {
        return await Mediator.Send(query, ct);
    }
    
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Summary/Daily")]
    public async Task<GetSummaryDailyResponse> GetSummaryDaily(
        [FromQuery] GetSummaryDailyQuery query, CancellationToken ct = default)
    {
        return await Mediator.Send(query, ct);
    }
    
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Summary/Network")]
    public async Task<GetSummaryByNetworkResponse> GetSummaryByNetwork(
        [FromQuery] GetSummaryByNetworkQuery query, CancellationToken ct = default)
    {
        return await Mediator.Send(query, ct);
    }
    
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Summary/File")]
    public async Task<GetSummaryByFileResponse> GetSummaryByFile(
        [FromQuery] GetSummaryByFileQuery query, CancellationToken ct = default)
    {
        return await Mediator.Send(query, ct);
    }
    
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Summary")]
    public async Task<GetSummaryOverallResponse> GetSummaryOverall(CancellationToken ct = default)
    {
        return await Mediator.Send(new GetSummaryOverallQuery(), ct);
    }
}
