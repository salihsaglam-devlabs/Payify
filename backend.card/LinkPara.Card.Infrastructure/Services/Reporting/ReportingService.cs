using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
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

    public async Task<PaginatedList<DailyTransactionVolumeDto>> GetDailyTransactionVolumeAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string currency,
        string financialType, string txnEffect, DateTime? dateFrom, DateTime? dateTo,
        string volumeFlag, string urgency, CancellationToken ct)
    {
        var q = _dbContext.DailyTransactionVolume.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(currency)) q = q.Where(x => x.Currency == currency);
        if (!string.IsNullOrWhiteSpace(financialType)) q = q.Where(x => x.FinancialType == financialType);
        if (!string.IsNullOrWhiteSpace(txnEffect)) q = q.Where(x => x.TxnEffect == txnEffect);
        if (dateFrom.HasValue) q = q.Where(x => x.TransactionDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(x => x.TransactionDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(volumeFlag)) q = q.Where(x => x.VolumeFlag == volumeFlag);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.TransactionDate).ThenBy(x => x.Network);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<MccRevenueConcentrationDto>> GetMccRevenueConcentrationAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string mcc,
        string concentrationRisk, string urgency, CancellationToken ct)
    {
        var q = _dbContext.MccRevenueConcentration.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(mcc)) q = q.Where(x => x.Mcc == mcc);
        if (!string.IsNullOrWhiteSpace(concentrationRisk)) q = q.Where(x => x.ConcentrationRisk == concentrationRisk);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.TotalAmount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<MerchantRiskHotspotDto>> GetMerchantRiskHotspotsAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string merchantId,
        string merchantCountry, string riskFlag, string urgency, CancellationToken ct)
    {
        var q = _dbContext.MerchantRiskHotspots.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(merchantId)) q = q.Where(x => x.MerchantId == merchantId);
        if (!string.IsNullOrWhiteSpace(merchantCountry)) q = q.Where(x => x.MerchantCountry == merchantCountry);
        if (!string.IsNullOrWhiteSpace(riskFlag)) q = q.Where(x => x.RiskFlag == riskFlag);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.UnmatchedAmount).ThenByDescending(x => x.UnmatchedRatePct);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<CountryCrossBorderExposureDto>> GetCountryCrossBorderExposureAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string merchantCountry,
        string fxPattern, string originalCurrency, string settlementCurrency,
        string exposureFlag, string urgency, CancellationToken ct)
    {
        var q = _dbContext.CountryCrossBorderExposure.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(merchantCountry)) q = q.Where(x => x.MerchantCountry == merchantCountry);
        if (!string.IsNullOrWhiteSpace(fxPattern)) q = q.Where(x => x.FxPattern == fxPattern);
        if (!string.IsNullOrWhiteSpace(originalCurrency)) q = q.Where(x => x.OriginalCurrency == originalCurrency);
        if (!string.IsNullOrWhiteSpace(settlementCurrency)) q = q.Where(x => x.SettlementCurrency == settlementCurrency);
        if (!string.IsNullOrWhiteSpace(exposureFlag)) q = q.Where(x => x.ExposureFlag == exposureFlag);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.TotalOriginalAmount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ResponseCodeDeclineHealthDto>> GetResponseCodeDeclineHealthAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string responseCode,
        string healthFlag, string urgency, CancellationToken ct)
    {
        var q = _dbContext.ResponseCodeDeclineHealth.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(responseCode)) q = q.Where(x => x.ResponseCode == responseCode);
        if (!string.IsNullOrWhiteSpace(healthFlag)) q = q.Where(x => x.HealthFlag == healthFlag);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.TransactionCount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<SettlementLagAnalysisDto>> GetSettlementLagAnalysisAsync(
        SearchQueryParams paging, DataScope? dataScope, string network,
        DateTime? dateFrom, DateTime? dateTo, string lagHealth, string urgency, CancellationToken ct)
    {
        var q = _dbContext.SettlementLagAnalysis.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (dateFrom.HasValue) q = q.Where(x => x.TransactionDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(x => x.TransactionDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(lagHealth)) q = q.Where(x => x.LagHealth == lagHealth);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.TransactionDate).ThenBy(x => x.Network);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<CurrencyFxDriftDto>> GetCurrencyFxDriftAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string originalCurrency,
        string settlementCurrency, string billingCurrency, string fxDriftSeverity,
        string urgency, CancellationToken ct)
    {
        var q = _dbContext.CurrencyFxDrift.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(originalCurrency)) q = q.Where(x => x.OriginalCurrency == originalCurrency);
        if (!string.IsNullOrWhiteSpace(settlementCurrency)) q = q.Where(x => x.SettlementCurrency == settlementCurrency);
        if (!string.IsNullOrWhiteSpace(billingCurrency)) q = q.Where(x => x.BillingCurrency == billingCurrency);
        if (!string.IsNullOrWhiteSpace(fxDriftSeverity)) q = q.Where(x => x.FxDriftSeverity == fxDriftSeverity);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.SettlementDrift);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<InstallmentPortfolioSummaryDto>> GetInstallmentPortfolioSummaryAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string installmentBucket,
        string portfolioFlag, string urgency, CancellationToken ct)
    {
        var q = _dbContext.InstallmentPortfolioSummary.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(installmentBucket)) q = q.Where(x => x.InstallmentBucket == installmentBucket);
        if (!string.IsNullOrWhiteSpace(portfolioFlag)) q = q.Where(x => x.PortfolioFlag == portfolioFlag);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderBy(x => x.Network).ThenByDescending(x => x.TotalAmount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<LoyaltyPointsEconomyDto>> GetLoyaltyPointsEconomyAsync(
        SearchQueryParams paging, DataScope? dataScope, string network,
        DateTime? dateFrom, DateTime? dateTo, string loyaltyIntensity, string urgency, CancellationToken ct)
    {
        var q = _dbContext.LoyaltyPointsEconomy.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (dateFrom.HasValue) q = q.Where(x => x.TransactionDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(x => x.TransactionDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(loyaltyIntensity)) q = q.Where(x => x.LoyaltyIntensity == loyaltyIntensity);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.TransactionDate).ThenBy(x => x.Network);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ClearingDisputeSummaryDto>> GetClearingDisputeSummaryAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string disputeCode,
        string reasonCode, string controlStat, string disputeFlag, string urgency,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.ClearingDisputeSummary.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(disputeCode)) q = q.Where(x => x.DisputeCode == disputeCode);
        if (!string.IsNullOrWhiteSpace(reasonCode)) q = q.Where(x => x.ReasonCode == reasonCode);
        if (!string.IsNullOrWhiteSpace(controlStat)) q = q.Where(x => x.ControlStat == controlStat);
        if (!string.IsNullOrWhiteSpace(disputeFlag)) q = q.Where(x => x.DisputeFlag == disputeFlag);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        if (dateFrom.HasValue) q = q.Where(x => x.LastTxnDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(x => x.LastTxnDate <= dateTo.Value);
        q = q.OrderByDescending(x => x.TotalReimbursementAmount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ClearingIoImbalanceDto>> GetClearingIoImbalanceAsync(
        SearchQueryParams paging, DataScope? dataScope, string network,
        DateTime? dateFrom, DateTime? dateTo, string imbalanceFlag, string urgency, CancellationToken ct)
    {
        var q = _dbContext.ClearingIoImbalance.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (dateFrom.HasValue) q = q.Where(x => x.TxnDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(x => x.TxnDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(imbalanceFlag)) q = q.Where(x => x.ImbalanceFlag == imbalanceFlag);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.TxnDate).ThenBy(x => x.Network);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<HighValueUnmatchedTransactionDto>> GetHighValueUnmatchedTransactionsAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string merchantCountry,
        string currency, decimal? minAmount, string riskFlag, string urgency,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct)
    {
        var q = _dbContext.HighValueUnmatchedTransactions.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(merchantCountry)) q = q.Where(x => x.MerchantCountry == merchantCountry);
        if (!string.IsNullOrWhiteSpace(currency)) q = q.Where(x => x.Currency == currency);
        if (minAmount.HasValue) q = q.Where(x => x.OriginalAmount >= minAmount.Value);
        if (!string.IsNullOrWhiteSpace(riskFlag)) q = q.Where(x => x.RiskFlag == riskFlag);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        if (dateFrom.HasValue) q = q.Where(x => x.TransactionDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(x => x.TransactionDate <= dateTo.Value);
        q = q.OrderByDescending(x => x.OriginalAmount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ActionRadarDto>> GetActionRadarAsync(
        SearchQueryParams paging, DataScope? dataScope, string category, string issueType,
        string urgency, CancellationToken ct)
    {
        var q = _dbContext.ActionRadar.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(x => x.Category == category);
        if (!string.IsNullOrWhiteSpace(issueType)) q = q.Where(x => x.IssueType == issueType);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderBy(x => x.Urgency).ThenByDescending(x => x.OpenCount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<UnhealthyFileDto>> GetUnhealthyFilesAsync(
        SearchQueryParams paging, DataScope? dataScope, string side, string network,
        string fileStatus, string issueCategory, string urgency, CancellationToken ct)
    {
        var q = _dbContext.UnhealthyFiles.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(side)) q = q.Where(x => x.Side == side);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(fileStatus)) q = q.Where(x => x.FileStatus == fileStatus);
        if (!string.IsNullOrWhiteSpace(issueCategory)) q = q.Where(x => x.IssueCategory == issueCategory);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderBy(x => x.Urgency).ThenByDescending(x => x.AgeHours);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<StuckPipelineItemDto>> GetStuckPipelineItemsAsync(
        SearchQueryParams paging, DataScope? dataScope, string stage, string stuckState,
        string urgency, CancellationToken ct)
    {
        var q = _dbContext.StuckPipelineItems.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(stage)) q = q.Where(x => x.Stage == stage);
        if (!string.IsNullOrWhiteSpace(stuckState)) q = q.Where(x => x.StuckState == stuckState);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderBy(x => x.Urgency).ThenByDescending(x => x.StuckMinutes);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ReconFailureCategorizationDto>> GetReconFailureCategorizationAsync(
        SearchQueryParams paging, DataScope? dataScope, string operationCode, string branch,
        string likelyRootCause, string urgency, CancellationToken ct)
    {
        var q = _dbContext.ReconFailureCategorization.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(operationCode)) q = q.Where(x => x.OperationCode == operationCode);
        if (!string.IsNullOrWhiteSpace(branch)) q = q.Where(x => x.Branch == branch);
        if (!string.IsNullOrWhiteSpace(likelyRootCause)) q = q.Where(x => x.LikelyRootCause == likelyRootCause);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderBy(x => x.Urgency).ThenByDescending(x => x.FailedCount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ManualReviewPressureDto>> GetManualReviewPressureAsync(
        SearchQueryParams paging, DataScope? dataScope, string slaBucket, string defaultOnExpiry,
        string currency, string urgency, CancellationToken ct)
    {
        var q = _dbContext.ManualReviewPressure.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(slaBucket)) q = q.Where(x => x.SlaBucket == slaBucket);
        if (!string.IsNullOrWhiteSpace(defaultOnExpiry)) q = q.Where(x => x.DefaultOnExpiry == defaultOnExpiry);
        if (!string.IsNullOrWhiteSpace(currency)) q = q.Where(x => x.Currency == currency);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderBy(x => x.Urgency).ThenByDescending(x => x.ExposureAmount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<AlertDeliveryHealthDto>> GetAlertDeliveryHealthAsync(
        SearchQueryParams paging, DataScope? dataScope, string severity, string alertType,
        string deliveryHealthStatus, string urgency, CancellationToken ct)
    {
        var q = _dbContext.AlertDeliveryHealth.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(severity)) q = q.Where(x => x.Severity == severity);
        if (!string.IsNullOrWhiteSpace(alertType)) q = q.Where(x => x.AlertType == alertType);
        if (!string.IsNullOrWhiteSpace(deliveryHealthStatus)) q = q.Where(x => x.DeliveryHealthStatus == deliveryHealthStatus);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderBy(x => x.Urgency).ThenByDescending(x => x.FailedCount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<UnmatchedFinancialExposureDto>> GetUnmatchedFinancialExposureAsync(
        SearchQueryParams paging, DataScope? dataScope, string side, string network, string currency,
        string agingBucket, string riskFlag, string urgency, CancellationToken ct)
    {
        var q = _dbContext.UnmatchedFinancialExposure.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(side)) q = q.Where(x => x.Side == side);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(currency)) q = q.Where(x => x.Currency == currency);
        if (!string.IsNullOrWhiteSpace(agingBucket)) q = q.Where(x => x.AgingBucket == agingBucket);
        if (!string.IsNullOrWhiteSpace(riskFlag)) q = q.Where(x => x.RiskFlag == riskFlag);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderBy(x => x.Urgency).ThenByDescending(x => x.ExposureAmount);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<CardClearingImbalanceDto>> GetCardClearingImbalanceAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string currency,
        DateTime? dateFrom, DateTime? dateTo, string imbalanceSeverity, string urgency, CancellationToken ct)
    {
        var q = _dbContext.CardClearingImbalance.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(currency)) q = q.Where(x => x.Currency == currency);
        if (dateFrom.HasValue) q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(x => x.ReportDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(imbalanceSeverity)) q = q.Where(x => x.ImbalanceSeverity == imbalanceSeverity);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.ReportDate).ThenByDescending(x => x.AbsGap);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ReconciliationQualityScoreDto>> GetReconciliationQualityScoreAsync(
        SearchQueryParams paging, DataScope? dataScope, string network,
        DateTime? dateFrom, DateTime? dateTo, string qualityGrade, string weakestDimension,
        string urgency, CancellationToken ct)
    {
        var q = _dbContext.ReconciliationQualityScore.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (dateFrom.HasValue) q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(x => x.ReportDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(qualityGrade)) q = q.Where(x => x.QualityGrade == qualityGrade);
        if (!string.IsNullOrWhiteSpace(weakestDimension)) q = q.Where(x => x.WeakestDimension == weakestDimension);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.ReportDate).ThenBy(x => x.Network);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<MisleadingSuccessCaseDto>> GetMisleadingSuccessCasesAsync(
        SearchQueryParams paging, DataScope? dataScope, string network, string side, string currency,
        DateTime? dateFrom, DateTime? dateTo, string misleadingPattern, string urgency, CancellationToken ct)
    {
        var q = _dbContext.MisleadingSuccessCases.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(side)) q = q.Where(x => x.Side == side);
        if (!string.IsNullOrWhiteSpace(currency)) q = q.Where(x => x.Currency == currency);
        if (dateFrom.HasValue) q = q.Where(x => x.ReportDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(x => x.ReportDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(misleadingPattern)) q = q.Where(x => x.MisleadingPattern == misleadingPattern);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderByDescending(x => x.ReportDate).ThenBy(x => x.Network);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ArchivePipelineHealthDto>> GetArchivePipelineHealthAsync(
        SearchQueryParams paging, DataScope? dataScope, string perspective, string side, string network,
        string archiveStatus, string pipelineHealth, string urgency, CancellationToken ct)
    {
        var q = _dbContext.ArchivePipelineHealth.AsNoTracking();
        if (dataScope.HasValue) q = q.Where(x => x.DataScope == dataScope.Value);
        if (!string.IsNullOrWhiteSpace(perspective)) q = q.Where(x => x.Perspective == perspective);
        if (!string.IsNullOrWhiteSpace(side)) q = q.Where(x => x.Side == side);
        if (!string.IsNullOrWhiteSpace(network)) q = q.Where(x => x.Network == network);
        if (!string.IsNullOrWhiteSpace(archiveStatus)) q = q.Where(x => x.ArchiveStatus == archiveStatus);
        if (!string.IsNullOrWhiteSpace(pipelineHealth)) q = q.Where(x => x.PipelineHealth == pipelineHealth);
        if (!string.IsNullOrWhiteSpace(urgency)) q = q.Where(x => x.Urgency == urgency);
        q = q.OrderBy(x => x.Urgency).ThenByDescending(x => x.AgeDays);
        return await PaginateAsync(q, paging, ct);
    }

    public async Task<PaginatedList<ReportingDocumentationDto>> GetReportingDocumentationAsync(
        SearchQueryParams paging, string viewName, string reportGroup, CancellationToken ct)
    {
        var q = _dbContext.ReportingDocumentation.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(viewName)) q = q.Where(x => x.ViewName == viewName);
        if (!string.IsNullOrWhiteSpace(reportGroup)) q = q.Where(x => x.ReportGroup == reportGroup);
        q = q.OrderBy(x => x.ReportGroup).ThenBy(x => x.ViewName);
        return await PaginateAsync(q, paging, ct);
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

