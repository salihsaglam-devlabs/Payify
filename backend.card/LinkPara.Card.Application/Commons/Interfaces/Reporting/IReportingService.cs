using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Domain.Enums.Reporting;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Card.Application.Commons.Interfaces.Reporting;

public interface IReportingService
{
    Task<PaginatedList<IngestionFileOverviewDto>> GetIngestionFileOverviewAsync(
        SearchQueryParams paging, DataScope? dataScope = null, FileContentType? contentType = null, FileType? fileType = null, FileStatus? fileStatus = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<PaginatedList<IngestionFileQualityDto>> GetIngestionFileQualityAsync(
        SearchQueryParams paging, DataScope? dataScope = null, FileContentType? contentType = null, FileType? fileType = null, FileStatus? fileStatus = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<List<IngestionDailySummaryDto>> GetIngestionDailySummaryAsync(
        DataScope? dataScope = null, FileContentType? contentType = null, FileType? fileType = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<List<IngestionNetworkMatrixDto>> GetIngestionNetworkMatrixAsync(DataScope? dataScope = null, CancellationToken ct = default);

    Task<PaginatedList<IngestionExceptionHotspotDto>> GetIngestionExceptionHotspotsAsync(
        SearchQueryParams paging, DataScope? dataScope = null, FileContentType? contentType = null, FileType? fileType = null, SeverityLevel? severityLevel = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);
    
    Task<List<ReconDailyOverviewDto>> GetReconDailyOverviewAsync(
        DataScope? dataScope = null, DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<PaginatedList<ReconOpenItemDto>> GetReconOpenItemsAsync(
        SearchQueryParams paging, OperationStatus? operationStatus = null, string branch = null, bool? isManual = null,
        CancellationToken ct = default);

    Task<List<ReconOpenItemAgingDto>> GetReconOpenItemAgingAsync(CancellationToken ct = default);

    Task<PaginatedList<ReconManualReviewQueueDto>> GetReconManualReviewQueueAsync(
        SearchQueryParams paging, UrgencyLevel? urgencyLevel = null, string operationBranch = null,
        CancellationToken ct = default);

    Task<List<ReconAlertSummaryDto>> GetReconAlertSummaryAsync(
        DataScope? dataScope = null, string severity = null, string alertType = null, AlertStatus? alertStatus = null, CancellationToken ct = default);
    
    Task<PaginatedList<ReconCardContentDailyDto>> GetReconLiveCardContentDailyAsync(
        SearchQueryParams paging, FileContentType? network = null, DateTime? dateFrom = null, DateTime? dateTo = null,
        CancellationToken ct = default);

    Task<PaginatedList<ReconClearingContentDailyDto>> GetReconLiveClearingContentDailyAsync(
        SearchQueryParams paging, FileContentType? network = null, DateTime? dateFrom = null, DateTime? dateTo = null,
        CancellationToken ct = default);

    Task<PaginatedList<ReconCardContentDailyDto>> GetReconArchiveCardContentDailyAsync(
        SearchQueryParams paging, FileContentType? network = null, DateTime? dateFrom = null, DateTime? dateTo = null,
        CancellationToken ct = default);

    Task<PaginatedList<ReconClearingContentDailyDto>> GetReconArchiveClearingContentDailyAsync(
        SearchQueryParams paging, FileContentType? network = null, DateTime? dateFrom = null, DateTime? dateTo = null,
        CancellationToken ct = default);

    Task<PaginatedList<ReconContentDailyDto>> GetReconContentDailyAsync(
        SearchQueryParams paging, DataScope? dataScope = null, FileContentType? network = null, ReconSide? side = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<List<ReconClearingControlStatAnalysisDto>> GetReconClearingControlStatAnalysisAsync(
        DataScope? dataScope = null, FileContentType? network = null, CancellationToken ct = default);

    Task<List<ReconFinancialSummaryDto>> GetReconFinancialSummaryAsync(
        DataScope? dataScope = null, FileContentType? network = null, string financialType = null, string txnEffect = null,
        int? originalCurrency = null, CancellationToken ct = default);

    Task<List<ReconResponseStatusAnalysisDto>> GetReconResponseStatusAnalysisAsync(
        DataScope? dataScope = null, FileContentType? network = null, ReconciliationStatus? reconciliationStatus = null,
        CancellationToken ct = default);
    
    Task<PaginatedList<ArchiveRunOverviewDto>> GetArchiveRunOverviewAsync(
        SearchQueryParams paging, string archiveStatus = null, FileContentType? contentType = null, FileType? fileType = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<PaginatedList<ArchiveEligibilityDto>> GetArchiveEligibilityAsync(
        SearchQueryParams paging, FileContentType? contentType = null, FileType? fileType = null,
        ArchiveEligibilityStatus? archiveEligibilityStatus = null, CancellationToken ct = default);

    Task<List<ArchiveBacklogTrendDto>> GetArchiveBacklogTrendAsync(
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<ArchiveRetentionSnapshotDto> GetArchiveRetentionSnapshotAsync(CancellationToken ct = default);

    Task<PaginatedList<FileReconSummaryDto>> GetFileReconSummaryAsync(
        SearchQueryParams paging, DataScope? dataScope = null, FileContentType? contentType = null, FileType? fileType = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<List<ReconMatchRateTrendDto>> GetReconMatchRateTrendAsync(
        DataScope? dataScope = null, FileContentType? network = null, ReconSide? side = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<List<ReconGapAnalysisDto>> GetReconGapAnalysisAsync(
        DataScope? dataScope = null, FileContentType? network = null,
        DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);

    Task<List<UnmatchedTransactionAgingDto>> GetUnmatchedTransactionAgingAsync(
        DataScope? dataScope = null, FileContentType? network = null, ReconSide? side = null,
        CancellationToken ct = default);

    Task<List<NetworkReconScorecardDto>> GetNetworkReconScorecardAsync(
        DataScope? dataScope = null, FileContentType? network = null, CancellationToken ct = default);
}
