using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Domain.Enums.Reporting;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.SharedModels.Pagination;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Reporting;

internal sealed class ReportingService : IReportingService
{
    private readonly CardDbContext _dbContext;

    public ReportingService(CardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedList<IngestionFileOverviewDto>> GetIngestionFileOverviewAsync(
        SearchQueryParams paging, DataScope? dataScope, FileContentType? contentType, FileType? fileType, FileStatus? fileStatus,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<IngestionFileOverviewDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (contentType.HasValue)
            q = q.Where(x => x.ContentType == contentType.Value);
        if (fileType.HasValue)
            q = q.Where(x => x.FileType == fileType.Value);
        if (fileStatus.HasValue)
            q = q.Where(x => x.FileStatus == fileStatus.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.FileCreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.FileCreatedAt <= dateTo.Value);

        q = q.OrderByDescending(x => x.FileCreatedAt);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<IngestionFileQualityDto>> GetIngestionFileQualityAsync(
        SearchQueryParams paging, DataScope? dataScope, FileContentType? contentType, FileType? fileType, FileStatus? fileStatus,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<IngestionFileQualityDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (contentType.HasValue)
            q = q.Where(x => x.ContentType == contentType.Value);
        if (fileType.HasValue)
            q = q.Where(x => x.FileType == fileType.Value);
        if (fileStatus.HasValue)
            q = q.Where(x => x.FileStatus == fileStatus.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.FileCreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.FileCreatedAt <= dateTo.Value);

        q = q.OrderByDescending(x => x.FileCreatedAt);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<List<IngestionDailySummaryDto>> GetIngestionDailySummaryAsync(
        DataScope? dataScope, FileContentType? contentType, FileType? fileType, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<IngestionDailySummaryDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (contentType.HasValue)
            q = q.Where(x => x.ContentType == contentType.Value);
        if (fileType.HasValue)
            q = q.Where(x => x.FileType == fileType.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        return await q.OrderByDescending(x => x.ReportDate).ToListAsync(ct);
    }

    public async Task<List<IngestionNetworkMatrixDto>> GetIngestionNetworkMatrixAsync(DataScope? dataScope, CancellationToken ct)
    {
        var q = _dbContext.Set<IngestionNetworkMatrixDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);

        return await q.OrderBy(x => x.ContentType).ThenBy(x => x.FileType)
            .ToListAsync(ct);
    }

    public async Task<PaginatedList<IngestionExceptionHotspotDto>> GetIngestionExceptionHotspotsAsync(
        SearchQueryParams paging, DataScope? dataScope, FileContentType? contentType, FileType? fileType, SeverityLevel? severityLevel,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<IngestionExceptionHotspotDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (contentType.HasValue)
            q = q.Where(x => x.ContentType == contentType.Value);
        if (fileType.HasValue)
            q = q.Where(x => x.FileType == fileType.Value);
        if (severityLevel.HasValue)
            q = q.Where(x => x.SeverityLevel == severityLevel.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.FileCreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.FileCreatedAt <= dateTo.Value);

        q = q.OrderByDescending(x => x.FileCreatedAt);
        return await PaginateAsync(q, paging, ct);
    }

 
    public async Task<List<ReconDailyOverviewDto>> GetReconDailyOverviewAsync(
        DataScope? dataScope, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconDailyOverviewDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        return await q.OrderByDescending(x => x.ReportDate).ToListAsync(ct);
    }

    public async Task<PaginatedList<ReconOpenItemDto>> GetReconOpenItemsAsync(
        SearchQueryParams paging, OperationStatus? operationStatus, string branch, bool? isManual, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconOpenItemDto>().AsNoTracking().AsQueryable();

        if (operationStatus.HasValue)
            q = q.Where(x => x.OperationStatus == operationStatus.Value);
        if (!string.IsNullOrWhiteSpace(branch))
            q = q.Where(x => x.Branch == branch);
        if (isManual.HasValue)
            q = q.Where(x => x.IsManual == isManual.Value);

        q = q.OrderByDescending(x => x.AgeHours);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<List<ReconOpenItemAgingDto>> GetReconOpenItemAgingAsync(CancellationToken ct)
    {
        return await _dbContext.Set<ReconOpenItemAgingDto>().AsNoTracking()
            .OrderBy(x => x.BucketName)
            .ToListAsync(ct);
    }

    public async Task<PaginatedList<ReconManualReviewQueueDto>> GetReconManualReviewQueueAsync(
        SearchQueryParams paging, UrgencyLevel? urgencyLevel, string operationBranch, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconManualReviewQueueDto>().AsNoTracking().AsQueryable();

        if (urgencyLevel.HasValue)
            q = q.Where(x => x.UrgencyLevel == urgencyLevel.Value);
        if (!string.IsNullOrWhiteSpace(operationBranch))
            q = q.Where(x => x.OperationBranch == operationBranch);

        q = q.OrderByDescending(x => x.WaitingHours);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<List<ReconAlertSummaryDto>> GetReconAlertSummaryAsync(
        DataScope? dataScope, string severity, string alertType, AlertStatus? alertStatus, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconAlertSummaryDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(severity))
            q = q.Where(x => x.Severity == severity);
        if (!string.IsNullOrWhiteSpace(alertType))
            q = q.Where(x => x.AlertType == alertType);
        if (alertStatus.HasValue)
            q = q.Where(x => x.AlertStatus == alertStatus.Value);

        return await q.OrderByDescending(x => x.AlertCount).ToListAsync(ct);
    }
    
    public async Task<PaginatedList<ReconCardContentDailyDto>> GetReconLiveCardContentDailyAsync(
        SearchQueryParams paging, FileContentType? network, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconCardContentDailyDto>().AsNoTracking().AsQueryable();

        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        q = q.OrderByDescending(x => x.ReportDate);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ReconClearingContentDailyDto>> GetReconLiveClearingContentDailyAsync(
        SearchQueryParams paging, FileContentType? network, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconClearingContentDailyDto>().AsNoTracking().AsQueryable();

        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        q = q.OrderByDescending(x => x.ReportDate);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ReconCardContentDailyDto>> GetReconArchiveCardContentDailyAsync(
        SearchQueryParams paging, FileContentType? network, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconCardContentDailyDto>()
            .FromSqlRaw("SELECT * FROM reporting.vw_recon_archive_card_content_daily")
            .AsNoTracking();

        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        q = q.OrderByDescending(x => x.ReportDate);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ReconClearingContentDailyDto>> GetReconArchiveClearingContentDailyAsync(
        SearchQueryParams paging, FileContentType? network, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconClearingContentDailyDto>()
            .FromSqlRaw("SELECT * FROM reporting.vw_recon_archive_clearing_content_daily")
            .AsNoTracking();

        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        q = q.OrderByDescending(x => x.ReportDate);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ReconContentDailyDto>> GetReconContentDailyAsync(
        SearchQueryParams paging, DataScope? dataScope, FileContentType? network, ReconSide? side,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconContentDailyDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (side.HasValue)
            q = q.Where(x => x.Side == side.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        q = q.OrderByDescending(x => x.ReportDate);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<List<ReconClearingControlStatAnalysisDto>> GetReconClearingControlStatAnalysisAsync(
        DataScope? dataScope, FileContentType? network, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconClearingControlStatAnalysisDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);

        return await q.OrderByDescending(x => x.TransactionCount).ToListAsync(ct);
    }

    public async Task<List<ReconFinancialSummaryDto>> GetReconFinancialSummaryAsync(
        DataScope? dataScope, FileContentType? network, string financialType, string txnEffect,
        int? originalCurrency, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconFinancialSummaryDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (!string.IsNullOrWhiteSpace(financialType))
            q = q.Where(x => x.FinancialType == financialType);
        if (!string.IsNullOrWhiteSpace(txnEffect))
            q = q.Where(x => x.TxnEffect == txnEffect);
        if (originalCurrency.HasValue)
            q = q.Where(x => x.OriginalCurrency == originalCurrency.Value);

        return await q.OrderByDescending(x => x.TransactionCount).ToListAsync(ct);
    }

    public async Task<List<ReconResponseStatusAnalysisDto>> GetReconResponseStatusAnalysisAsync(
        DataScope? dataScope, FileContentType? network, ReconciliationStatus? reconciliationStatus, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconResponseStatusAnalysisDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (reconciliationStatus.HasValue)
            q = q.Where(x => x.ReconciliationStatus == reconciliationStatus.Value);

        return await q.OrderByDescending(x => x.TransactionCount).ToListAsync(ct);
    }
    
    public async Task<PaginatedList<ArchiveRunOverviewDto>> GetArchiveRunOverviewAsync(
        SearchQueryParams paging, string archiveStatus, FileContentType? contentType, FileType? fileType,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ArchiveRunOverviewDto>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(archiveStatus))
            q = q.Where(x => x.ArchiveStatus == archiveStatus);
        if (contentType.HasValue)
            q = q.Where(x => x.ContentType == contentType.Value);
        if (fileType.HasValue)
            q = q.Where(x => x.FileType == fileType.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ArchiveStartedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ArchiveStartedAt <= dateTo.Value);

        q = q.OrderByDescending(x => x.ArchiveStartedAt);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ArchiveEligibilityDto>> GetArchiveEligibilityAsync(
        SearchQueryParams paging, FileContentType? contentType, FileType? fileType,
        ArchiveEligibilityStatus? archiveEligibilityStatus, CancellationToken ct)
    {
        var q = _dbContext.Set<ArchiveEligibilityDto>().AsNoTracking().AsQueryable();

        if (contentType.HasValue)
            q = q.Where(x => x.ContentType == contentType.Value);
        if (fileType.HasValue)
            q = q.Where(x => x.FileType == fileType.Value);
        if (archiveEligibilityStatus.HasValue)
            q = q.Where(x => x.ArchiveEligibilityStatus == archiveEligibilityStatus.Value);

        q = q.OrderByDescending(x => x.AgeDays);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<List<ArchiveBacklogTrendDto>> GetArchiveBacklogTrendAsync(
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ArchiveBacklogTrendDto>().AsNoTracking().AsQueryable();

        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        return await q.OrderByDescending(x => x.ReportDate).ToListAsync(ct);
    }

    public async Task<ArchiveRetentionSnapshotDto> GetArchiveRetentionSnapshotAsync(CancellationToken ct)
    {
        return await _dbContext.Set<ArchiveRetentionSnapshotDto>().AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaginatedList<FileReconSummaryDto>> GetFileReconSummaryAsync(
        SearchQueryParams paging, DataScope? dataScope, FileContentType? contentType, FileType? fileType,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<FileReconSummaryDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (contentType.HasValue)
            q = q.Where(x => x.ContentType == contentType.Value);
        if (fileType.HasValue)
            q = q.Where(x => x.FileType == fileType.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.FileCreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.FileCreatedAt <= dateTo.Value);

        q = q.OrderByDescending(x => x.FileCreatedAt);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<List<ReconMatchRateTrendDto>> GetReconMatchRateTrendAsync(
        DataScope? dataScope, FileContentType? network, ReconSide? side,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconMatchRateTrendDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (side.HasValue)
            q = q.Where(x => x.Side == side.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        return await q.OrderByDescending(x => x.ReportDate).ToListAsync(ct);
    }

    public async Task<List<ReconGapAnalysisDto>> GetReconGapAnalysisAsync(
        DataScope? dataScope, FileContentType? network,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.Set<ReconGapAnalysisDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (dateFrom.HasValue)
            q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue)
            q = q.Where(x => x.ReportDate <= dateTo.Value);

        return await q.OrderByDescending(x => x.ReportDate).ToListAsync(ct);
    }

    public async Task<List<UnmatchedTransactionAgingDto>> GetUnmatchedTransactionAgingAsync(
        DataScope? dataScope, FileContentType? network, ReconSide? side, CancellationToken ct)
    {
        var q = _dbContext.Set<UnmatchedTransactionAgingDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);
        if (side.HasValue)
            q = q.Where(x => x.Side == side.Value);

        return await q.OrderBy(x => x.AgeBucket).ToListAsync(ct);
    }

    public async Task<List<NetworkReconScorecardDto>> GetNetworkReconScorecardAsync(
        DataScope? dataScope, FileContentType? network, CancellationToken ct)
    {
        var q = _dbContext.Set<NetworkReconScorecardDto>().AsNoTracking().AsQueryable();

        if (dataScope.HasValue)
            q = q.Where(x => x.DataScope == dataScope.Value);
        if (network.HasValue)
            q = q.Where(x => x.Network == network.Value);

        return await q.OrderBy(x => x.Network).ToListAsync(ct);
    }
    
    private static async Task<PaginatedList<T>> PaginateAsync<T>(
        IQueryable<T> query, SearchQueryParams paging, CancellationToken ct)
    {
        var page = Math.Max(paging.Page, 1);
        var pageSize = Math.Clamp(paging.Size, 1, 1000);
        var skip = (page - 1) * pageSize;

        var total = await query.CountAsync(ct);
        var items = await query.Skip(skip).Take(pageSize).ToListAsync(ct);

        return new PaginatedList<T>(items, total, page, pageSize, paging.OrderBy, paging.SortBy);
    }
}

