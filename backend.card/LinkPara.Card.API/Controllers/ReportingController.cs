using LinkPara.Card.Application.Commons.Models.Reporting;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;
using LinkPara.Card.Application.Features.Reporting.Queries.Documentation;
using LinkPara.Card.Application.Features.Reporting.Queries.Dynamic;
using LinkPara.Card.Application.Features.Reporting.Queries.Financial;
using LinkPara.Card.Application.Features.Reporting.Queries.Operational;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers;

[Route("v1/Reporting")]
public class ReportingController : ApiControllerBase
{
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/DailyTransactionVolume")]
    public async Task<GetDailyTransactionVolumeResponse> GetDailyTransactionVolume(
        [FromQuery] GetDailyTransactionVolumeQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/MccRevenueConcentration")]
    public async Task<GetMccRevenueConcentrationResponse> GetMccRevenueConcentration(
        [FromQuery] GetMccRevenueConcentrationQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/MerchantRiskHotspots")]
    public async Task<GetMerchantRiskHotspotsResponse> GetMerchantRiskHotspots(
        [FromQuery] GetMerchantRiskHotspotsQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/CountryCrossBorderExposure")]
    public async Task<GetCountryCrossBorderExposureResponse> GetCountryCrossBorderExposure(
        [FromQuery] GetCountryCrossBorderExposureQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/ResponseCodeDeclineHealth")]
    public async Task<GetResponseCodeDeclineHealthResponse> GetResponseCodeDeclineHealth(
        [FromQuery] GetResponseCodeDeclineHealthQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/SettlementLagAnalysis")]
    public async Task<GetSettlementLagAnalysisResponse> GetSettlementLagAnalysis(
        [FromQuery] GetSettlementLagAnalysisQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/CurrencyFxDrift")]
    public async Task<GetCurrencyFxDriftResponse> GetCurrencyFxDrift(
        [FromQuery] GetCurrencyFxDriftQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/InstallmentPortfolioSummary")]
    public async Task<GetInstallmentPortfolioSummaryResponse> GetInstallmentPortfolioSummary(
        [FromQuery] GetInstallmentPortfolioSummaryQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/LoyaltyPointsEconomy")]
    public async Task<GetLoyaltyPointsEconomyResponse> GetLoyaltyPointsEconomy(
        [FromQuery] GetLoyaltyPointsEconomyQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/ClearingDisputeSummary")]
    public async Task<GetClearingDisputeSummaryResponse> GetClearingDisputeSummary(
        [FromQuery] GetClearingDisputeSummaryQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/ClearingIoImbalance")]
    public async Task<GetClearingIoImbalanceResponse> GetClearingIoImbalance(
        [FromQuery] GetClearingIoImbalanceQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Financial/HighValueUnmatchedTransactions")]
    public async Task<GetHighValueUnmatchedTransactionsResponse> GetHighValueUnmatchedTransactions(
        [FromQuery] GetHighValueUnmatchedTransactionsQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);
    
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/ActionRadar")]
    public async Task<GetActionRadarResponse> GetActionRadar(
        [FromQuery] GetActionRadarQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/UnhealthyFiles")]
    public async Task<GetUnhealthyFilesResponse> GetUnhealthyFiles(
        [FromQuery] GetUnhealthyFilesQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/StuckPipelineItems")]
    public async Task<GetStuckPipelineItemsResponse> GetStuckPipelineItems(
        [FromQuery] GetStuckPipelineItemsQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/ReconFailureCategorization")]
    public async Task<GetReconFailureCategorizationResponse> GetReconFailureCategorization(
        [FromQuery] GetReconFailureCategorizationQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/ManualReviewPressure")]
    public async Task<GetManualReviewPressureResponse> GetManualReviewPressure(
        [FromQuery] GetManualReviewPressureQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/AlertDeliveryHealth")]
    public async Task<GetAlertDeliveryHealthResponse> GetAlertDeliveryHealth(
        [FromQuery] GetAlertDeliveryHealthQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/UnmatchedFinancialExposure")]
    public async Task<GetUnmatchedFinancialExposureResponse> GetUnmatchedFinancialExposure(
        [FromQuery] GetUnmatchedFinancialExposureQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/CardClearingImbalance")]
    public async Task<GetCardClearingImbalanceResponse> GetCardClearingImbalance(
        [FromQuery] GetCardClearingImbalanceQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/ReconciliationQualityScore")]
    public async Task<GetReconciliationQualityScoreResponse> GetReconciliationQualityScore(
        [FromQuery] GetReconciliationQualityScoreQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/MisleadingSuccessCases")]
    public async Task<GetMisleadingSuccessCasesResponse> GetMisleadingSuccessCases(
        [FromQuery] GetMisleadingSuccessCasesQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Operational/ArchivePipelineHealth")]
    public async Task<GetArchivePipelineHealthResponse> GetArchivePipelineHealth(
        [FromQuery] GetArchivePipelineHealthQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);

    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpGet("Documentation")]
    public async Task<GetReportingDocumentationResponse> GetReportingDocumentation(
        [FromQuery] GetReportingDocumentationQuery query, CancellationToken ct = default)
        => await Mediator.Send(query, ct);
    
    [Authorize(Policy = ReportingPolicies.Read)]
    [HttpPost("Dynamic")]
    public async Task<DynamicReportingResponse> GetDynamicReporting(
        [FromBody] GetDynamicReportingQuery query, CancellationToken ct = default)
        => await Mediator.Send(query ?? new GetDynamicReportingQuery(), ct);
}
