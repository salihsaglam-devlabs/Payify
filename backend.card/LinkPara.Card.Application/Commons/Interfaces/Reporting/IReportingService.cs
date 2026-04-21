using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using LinkPara.Card.Domain.Enums.Reporting;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Card.Application.Commons.Interfaces.Reporting;

public interface IReportingService
{
    Task<PaginatedList<DailyTransactionVolumeDto>> GetDailyTransactionVolumeAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string currency = null,
        string financialType = null, string txnEffect = null, DateTime? dateFrom = null, DateTime? dateTo = null,
        string volumeFlag = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<MccRevenueConcentrationDto>> GetMccRevenueConcentrationAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string mcc = null,
        string concentrationRisk = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<MerchantRiskHotspotDto>> GetMerchantRiskHotspotsAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string merchantId = null,
        string merchantCountry = null, string riskFlag = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<CountryCrossBorderExposureDto>> GetCountryCrossBorderExposureAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string merchantCountry = null,
        string fxPattern = null, string originalCurrency = null, string settlementCurrency = null,
        string exposureFlag = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<ResponseCodeDeclineHealthDto>> GetResponseCodeDeclineHealthAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string responseCode = null,
        string healthFlag = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<SettlementLagAnalysisDto>> GetSettlementLagAnalysisAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, string lagHealth = null, string urgency = null,
        CancellationToken ct = default);

    Task<PaginatedList<CurrencyFxDriftDto>> GetCurrencyFxDriftAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string originalCurrency = null,
        string settlementCurrency = null, string billingCurrency = null, string fxDriftSeverity = null,
        string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<InstallmentPortfolioSummaryDto>> GetInstallmentPortfolioSummaryAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string installmentBucket = null,
        string portfolioFlag = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<LoyaltyPointsEconomyDto>> GetLoyaltyPointsEconomyAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, string loyaltyIntensity = null, string urgency = null,
        CancellationToken ct = default);

    Task<PaginatedList<ClearingDisputeSummaryDto>> GetClearingDisputeSummaryAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string disputeCode = null,
        string reasonCode = null, string controlStat = null, string disputeFlag = null, string urgency = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<PaginatedList<ClearingIoImbalanceDto>> GetClearingIoImbalanceAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, string imbalanceFlag = null, string urgency = null,
        CancellationToken ct = default);

    Task<PaginatedList<HighValueUnmatchedTransactionDto>> GetHighValueUnmatchedTransactionsAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string merchantCountry = null,
        string currency = null, decimal? minAmount = null, string riskFlag = null, string urgency = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);
    
    Task<PaginatedList<ActionRadarDto>> GetActionRadarAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string category = null,
        string issueType = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<UnhealthyFileDto>> GetUnhealthyFilesAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string side = null, string network = null,
        string fileStatus = null, string issueCategory = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<StuckPipelineItemDto>> GetStuckPipelineItemsAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string stage = null,
        string stuckState = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<ReconFailureCategorizationDto>> GetReconFailureCategorizationAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string operationCode = null, string branch = null,
        string likelyRootCause = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<ManualReviewPressureDto>> GetManualReviewPressureAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string slaBucket = null,
        string defaultOnExpiry = null, string currency = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<AlertDeliveryHealthDto>> GetAlertDeliveryHealthAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string severity = null, string alertType = null,
        string deliveryHealthStatus = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<UnmatchedFinancialExposureDto>> GetUnmatchedFinancialExposureAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string side = null, string network = null,
        string currency = null, string agingBucket = null, string riskFlag = null, string urgency = null,
        CancellationToken ct = default);

    Task<PaginatedList<CardClearingImbalanceDto>> GetCardClearingImbalanceAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string currency = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, string imbalanceSeverity = null,
        string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<ReconciliationQualityScoreDto>> GetReconciliationQualityScoreAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, string qualityGrade = null,
        string weakestDimension = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<MisleadingSuccessCaseDto>> GetMisleadingSuccessCasesAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string network = null, string side = null,
        string currency = null, DateTime? dateFrom = null, DateTime? dateTo = null,
        string misleadingPattern = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<ArchivePipelineHealthDto>> GetArchivePipelineHealthAsync(
        SearchQueryParams paging, DataScope? dataScope = null, string perspective = null,
        string side = null, string network = null, string archiveStatus = null,
        string pipelineHealth = null, string urgency = null, CancellationToken ct = default);

    Task<PaginatedList<ReportingDocumentationDto>> GetReportingDocumentationAsync(
        SearchQueryParams paging, string viewName = null, string reportGroup = null,
        CancellationToken ct = default);
}

