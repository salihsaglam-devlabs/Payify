using LinkPara.Card.Application.Commons.Models.Reporting;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Application.Features.Reporting.Queries.Archive;
using LinkPara.Card.Application.Features.Reporting.Queries.Ingestion;
using LinkPara.Card.Application.Features.Reporting.Queries.ReconContent;
using LinkPara.Card.Application.Features.Reporting.Queries.ReconProcess;
using LinkPara.Card.Application.Features.Reporting.Queries.ReconAdvanced;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers;

[Route("v1/Reporting")]
public class ReportingController : ApiControllerBase
{

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Ingestion/FileOverview")]
    public async Task<GetIngestionFileOverviewResponse> GetIngestionFileOverview(
        [FromQuery] GetIngestionFileOverviewQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Ingestion/FileQuality")]
    public async Task<GetIngestionFileQualityResponse> GetIngestionFileQuality(
        [FromQuery] GetIngestionFileQualityQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Ingestion/DailySummary")]
    public async Task<GetIngestionDailySummaryResponse> GetIngestionDailySummary(
        [FromQuery] GetIngestionDailySummaryQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Ingestion/NetworkMatrix")]
    public async Task<GetIngestionNetworkMatrixResponse> GetIngestionNetworkMatrix(
        [FromQuery] GetIngestionNetworkMatrixQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Ingestion/ExceptionHotspots")]
    public async Task<GetIngestionExceptionHotspotsResponse> GetIngestionExceptionHotspots(
        [FromQuery] GetIngestionExceptionHotspotsQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);



    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/DailyOverview")]
    public async Task<GetReconDailyOverviewResponse> GetReconDailyOverview(
        [FromQuery] GetReconDailyOverviewQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/OpenItems")]
    public async Task<GetReconOpenItemsResponse> GetReconOpenItems(
        [FromQuery] GetReconOpenItemsQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/OpenItemAging")]
    public async Task<GetReconOpenItemAgingResponse> GetReconOpenItemAging(CancellationToken ct = default)
        => await Mediator.Send(new GetReconOpenItemAgingQuery(), ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/ManualReviewQueue")]
    public async Task<GetReconManualReviewQueueResponse> GetReconManualReviewQueue(
        [FromQuery] GetReconManualReviewQueueQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/AlertSummary")]
    public async Task<GetReconAlertSummaryResponse> GetReconAlertSummary(
        [FromQuery] GetReconAlertSummaryQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/LiveCardContentDaily")]
    public async Task<GetReconCardContentDailyResponse> GetReconLiveCardContentDaily(
        [FromQuery] GetReconLiveCardContentDailyQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/LiveClearingContentDaily")]
    public async Task<GetReconClearingContentDailyResponse> GetReconLiveClearingContentDaily(
        [FromQuery] GetReconLiveClearingContentDailyQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/ArchiveCardContentDaily")]
    public async Task<GetReconCardContentDailyResponse> GetReconArchiveCardContentDaily(
        [FromQuery] GetReconArchiveCardContentDailyQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/ArchiveClearingContentDaily")]
    public async Task<GetReconClearingContentDailyResponse> GetReconArchiveClearingContentDaily(
        [FromQuery] GetReconArchiveClearingContentDailyQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/ContentDaily")]
    public async Task<GetReconContentDailyResponse> GetReconContentDaily(
        [FromQuery] GetReconContentDailyQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/ClearingControlStatAnalysis")]
    public async Task<GetReconClearingControlStatAnalysisResponse> GetReconClearingControlStatAnalysis(
        [FromQuery] GetReconClearingControlStatAnalysisQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/FinancialSummary")]
    public async Task<GetReconFinancialSummaryResponse> GetReconFinancialSummary(
        [FromQuery] GetReconFinancialSummaryQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/ResponseStatusAnalysis")]
    public async Task<GetReconResponseStatusAnalysisResponse> GetReconResponseStatusAnalysis(
        [FromQuery] GetReconResponseStatusAnalysisQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Archive/RunOverview")]
    public async Task<GetArchiveRunOverviewResponse> GetArchiveRunOverview(
        [FromQuery] GetArchiveRunOverviewQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Archive/Eligibility")]
    public async Task<GetArchiveEligibilityResponse> GetArchiveEligibility(
        [FromQuery] GetArchiveEligibilityQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Archive/BacklogTrend")]
    public async Task<GetArchiveBacklogTrendResponse> GetArchiveBacklogTrend(
        [FromQuery] GetArchiveBacklogTrendQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Archive/RetentionSnapshot")]
    public async Task<GetArchiveRetentionSnapshotResponse> GetArchiveRetentionSnapshot(CancellationToken ct = default)
        => await Mediator.Send(new GetArchiveRetentionSnapshotQuery(), ct);



    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/FileReconSummary")]
    public async Task<GetFileReconSummaryResponse> GetFileReconSummary(
        [FromQuery] GetFileReconSummaryQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/MatchRateTrend")]
    public async Task<GetReconMatchRateTrendResponse> GetReconMatchRateTrend(
        [FromQuery] GetReconMatchRateTrendQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/GapAnalysis")]
    public async Task<GetReconGapAnalysisResponse> GetReconGapAnalysis(
        [FromQuery] GetReconGapAnalysisQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/UnmatchedTransactionAging")]
    public async Task<GetUnmatchedTransactionAgingResponse> GetUnmatchedTransactionAging(
        [FromQuery] GetUnmatchedTransactionAgingQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Reconciliation/NetworkScorecard")]
    public async Task<GetNetworkReconScorecardResponse> GetNetworkReconScorecard(
        [FromQuery] GetNetworkReconScorecardQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

}
