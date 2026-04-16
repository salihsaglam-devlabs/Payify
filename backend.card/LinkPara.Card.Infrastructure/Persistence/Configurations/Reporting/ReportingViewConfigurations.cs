using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reporting;

#region A. File Ingestion

public class IngestionFileOverviewViewConfiguration : IEntityTypeConfiguration<IngestionFileOverviewDto>
{
    public void Configure(EntityTypeBuilder<IngestionFileOverviewDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_ingestion_file_overview", "reporting");
        builder.Property(x => x.SourceType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ContentType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class IngestionFileQualityViewConfiguration : IEntityTypeConfiguration<IngestionFileQualityDto>
{
    public void Configure(EntityTypeBuilder<IngestionFileQualityDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_ingestion_file_quality", "reporting");
        builder.Property(x => x.FileType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ContentType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class IngestionDailySummaryViewConfiguration : IEntityTypeConfiguration<IngestionDailySummaryDto>
{
    public void Configure(EntityTypeBuilder<IngestionDailySummaryDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_ingestion_daily_summary", "reporting");
        builder.Property(x => x.ContentType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class IngestionNetworkMatrixViewConfiguration : IEntityTypeConfiguration<IngestionNetworkMatrixDto>
{
    public void Configure(EntityTypeBuilder<IngestionNetworkMatrixDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_ingestion_network_matrix", "reporting");
        builder.Property(x => x.ContentType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class IngestionExceptionHotspotViewConfiguration : IEntityTypeConfiguration<IngestionExceptionHotspotDto>
{
    public void Configure(EntityTypeBuilder<IngestionExceptionHotspotDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_ingestion_exception_hotspots", "reporting");
        builder.Property(x => x.SourceType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ContentType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.SeverityLevel).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

#endregion

#region B. Reconciliation Process

public class ReconDailyOverviewViewConfiguration : IEntityTypeConfiguration<ReconDailyOverviewDto>
{
    public void Configure(EntityTypeBuilder<ReconDailyOverviewDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_daily_overview", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ReconOpenItemViewConfiguration : IEntityTypeConfiguration<ReconOpenItemDto>
{
    public void Configure(EntityTypeBuilder<ReconOpenItemDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_open_items", "reporting");
        builder.Property(x => x.OperationStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.EvaluationStatus).HasConversion<string>().HasColumnType("text");
    }
}

public class ReconOpenItemAgingViewConfiguration : IEntityTypeConfiguration<ReconOpenItemAgingDto>
{
    public void Configure(EntityTypeBuilder<ReconOpenItemAgingDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_open_item_aging", "reporting");
    }
}

public class ReconManualReviewQueueViewConfiguration : IEntityTypeConfiguration<ReconManualReviewQueueDto>
{
    public void Configure(EntityTypeBuilder<ReconManualReviewQueueDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_manual_review_queue", "reporting");
        builder.Property(x => x.Decision).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ExpirationAction).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ExpirationFlowAction).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.OperationStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.UrgencyLevel).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.EvaluationStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileSourceType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ContentType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.LineStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.LineReconciliationStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.LineDuplicateStatus).HasConversion<string>().HasColumnType("text");
        
        builder.Property(x => x.LineNumber).HasColumnType("integer");
        builder.Property(x => x.CardTransactionDate).HasColumnType("integer");
        builder.Property(x => x.CardTransactionTime).HasColumnType("integer");
        builder.Property(x => x.CardOriginalCurrency).HasColumnType("integer");
        builder.Property(x => x.ClearingTxnDate).HasColumnType("integer");
        builder.Property(x => x.ClearingTxnTime).HasColumnType("integer");
        builder.Property(x => x.ClearingSourceCurrency).HasColumnType("integer");
        builder.Property(x => x.LastAttemptNumber).HasColumnType("integer");
        builder.Property(x => x.OperationRetryCount).HasColumnType("integer");
        builder.Property(x => x.OperationMaxRetries).HasColumnType("integer");
        builder.Property(x => x.EvaluationOperationCount).HasColumnType("integer");
    }
}

public class ReconAlertSummaryViewConfiguration : IEntityTypeConfiguration<ReconAlertSummaryDto>
{
    public void Configure(EntityTypeBuilder<ReconAlertSummaryDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_alert_summary", "reporting");
        builder.Property(x => x.AlertStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

#endregion

#region C. Reconciliation Content + Financial

public class ReconCardContentDailyViewConfiguration : IEntityTypeConfiguration<ReconCardContentDailyDto>
{
    public void Configure(EntityTypeBuilder<ReconCardContentDailyDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_live_card_content_daily", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.LineStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ReconciliationStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
        
        builder.Property(x => x.OriginalCurrency).HasColumnType("integer");
    }
}

public class ReconClearingContentDailyViewConfiguration : IEntityTypeConfiguration<ReconClearingContentDailyDto>
{
    public void Configure(EntityTypeBuilder<ReconClearingContentDailyDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_live_clearing_content_daily", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.LineStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ReconciliationStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
        
        builder.Property(x => x.SourceCurrency).HasColumnType("integer");
    }
}

public class ReconContentDailyViewConfiguration : IEntityTypeConfiguration<ReconContentDailyDto>
{
    public void Configure(EntityTypeBuilder<ReconContentDailyDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_content_daily", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.LineStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ReconciliationStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.Side).HasConversion<string>().HasColumnType("text");
    }
}

public class ReconClearingControlStatAnalysisViewConfiguration : IEntityTypeConfiguration<ReconClearingControlStatAnalysisDto>
{
    public void Configure(EntityTypeBuilder<ReconClearingControlStatAnalysisDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_clearing_controlstat_analysis", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.LineStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ReconFinancialSummaryViewConfiguration : IEntityTypeConfiguration<ReconFinancialSummaryDto>
{
    public void Configure(EntityTypeBuilder<ReconFinancialSummaryDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_financial_summary", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.LineStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
        
        builder.Property(x => x.OriginalCurrency).HasColumnType("integer");
    }
}

public class ReconResponseStatusAnalysisViewConfiguration : IEntityTypeConfiguration<ReconResponseStatusAnalysisDto>
{
    public void Configure(EntityTypeBuilder<ReconResponseStatusAnalysisDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_response_status_analysis", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.LineStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ReconciliationStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

#endregion

#region D. Archive

public class ArchiveRunOverviewViewConfiguration : IEntityTypeConfiguration<ArchiveRunOverviewDto>
{
    public void Configure(EntityTypeBuilder<ArchiveRunOverviewDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_archive_run_overview", "reporting");
        builder.Property(x => x.FileType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ContentType).HasConversion<string>().HasColumnType("text");
    }
}

public class ArchiveEligibilityViewConfiguration : IEntityTypeConfiguration<ArchiveEligibilityDto>
{
    public void Configure(EntityTypeBuilder<ArchiveEligibilityDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_archive_eligibility", "reporting");
        builder.Property(x => x.FileType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ContentType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ArchiveEligibilityStatus).HasConversion<string>().HasColumnType("text");
    }
}

public class ArchiveBacklogTrendViewConfiguration : IEntityTypeConfiguration<ArchiveBacklogTrendDto>
{
    public void Configure(EntityTypeBuilder<ArchiveBacklogTrendDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_archive_backlog_trend", "reporting");
    }
}

public class ArchiveRetentionSnapshotViewConfiguration : IEntityTypeConfiguration<ArchiveRetentionSnapshotDto>
{
    public void Configure(EntityTypeBuilder<ArchiveRetentionSnapshotDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_archive_retention_snapshot", "reporting");
    }
}

#endregion

#region E. Advanced Reconciliation Reports

public class FileReconSummaryViewConfiguration : IEntityTypeConfiguration<FileReconSummaryDto>
{
    public void Configure(EntityTypeBuilder<FileReconSummaryDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_file_recon_summary", "reporting");
        builder.Property(x => x.FileType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.ContentType).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.FileStatus).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ReconMatchRateTrendViewConfiguration : IEntityTypeConfiguration<ReconMatchRateTrendDto>
{
    public void Configure(EntityTypeBuilder<ReconMatchRateTrendDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_match_rate_trend", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.Side).HasConversion<string>().HasColumnType("text");
    }
}

public class ReconGapAnalysisViewConfiguration : IEntityTypeConfiguration<ReconGapAnalysisDto>
{
    public void Configure(EntityTypeBuilder<ReconGapAnalysisDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_recon_gap_analysis", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class UnmatchedTransactionAgingViewConfiguration : IEntityTypeConfiguration<UnmatchedTransactionAgingDto>
{
    public void Configure(EntityTypeBuilder<UnmatchedTransactionAgingDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_unmatched_transaction_aging", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.Side).HasConversion<string>().HasColumnType("text");
    }
}

public class NetworkReconScorecardViewConfiguration : IEntityTypeConfiguration<NetworkReconScorecardDto>
{
    public void Configure(EntityTypeBuilder<NetworkReconScorecardDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_network_recon_scorecard", "reporting");
        builder.Property(x => x.Network).HasConversion<string>().HasColumnType("text");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

#endregion

