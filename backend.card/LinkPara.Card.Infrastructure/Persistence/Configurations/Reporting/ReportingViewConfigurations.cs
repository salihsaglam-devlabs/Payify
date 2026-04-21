using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reporting;

public class ActionRadarViewConfiguration : IEntityTypeConfiguration<ActionRadarDto>
{
    public void Configure(EntityTypeBuilder<ActionRadarDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_action_radar", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class UnhealthyFilesViewConfiguration : IEntityTypeConfiguration<UnhealthyFileDto>
{
    public void Configure(EntityTypeBuilder<UnhealthyFileDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_unhealthy_files", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class StuckPipelineItemsViewConfiguration : IEntityTypeConfiguration<StuckPipelineItemDto>
{
    public void Configure(EntityTypeBuilder<StuckPipelineItemDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_stuck_pipeline_items", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ReconFailureCategorizationViewConfiguration : IEntityTypeConfiguration<ReconFailureCategorizationDto>
{
    public void Configure(EntityTypeBuilder<ReconFailureCategorizationDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_recon_failure_categorization", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ManualReviewPressureViewConfiguration : IEntityTypeConfiguration<ManualReviewPressureDto>
{
    public void Configure(EntityTypeBuilder<ManualReviewPressureDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_manual_review_pressure", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class AlertDeliveryHealthViewConfiguration : IEntityTypeConfiguration<AlertDeliveryHealthDto>
{
    public void Configure(EntityTypeBuilder<AlertDeliveryHealthDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_alert_delivery_health", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class UnmatchedFinancialExposureViewConfiguration : IEntityTypeConfiguration<UnmatchedFinancialExposureDto>
{
    public void Configure(EntityTypeBuilder<UnmatchedFinancialExposureDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_unmatched_financial_exposure", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class CardClearingImbalanceViewConfiguration : IEntityTypeConfiguration<CardClearingImbalanceDto>
{
    public void Configure(EntityTypeBuilder<CardClearingImbalanceDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_card_clearing_imbalance", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ReconciliationQualityScoreViewConfiguration : IEntityTypeConfiguration<ReconciliationQualityScoreDto>
{
    public void Configure(EntityTypeBuilder<ReconciliationQualityScoreDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_reconciliation_quality_score", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class MisleadingSuccessCasesViewConfiguration : IEntityTypeConfiguration<MisleadingSuccessCaseDto>
{
    public void Configure(EntityTypeBuilder<MisleadingSuccessCaseDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_misleading_success_cases", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ArchivePipelineHealthViewConfiguration : IEntityTypeConfiguration<ArchivePipelineHealthDto>
{
    public void Configure(EntityTypeBuilder<ArchivePipelineHealthDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_archive_pipeline_health", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class DailyTransactionVolumeViewConfiguration : IEntityTypeConfiguration<DailyTransactionVolumeDto>
{
    public void Configure(EntityTypeBuilder<DailyTransactionVolumeDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_daily_transaction_volume", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class MccRevenueConcentrationViewConfiguration : IEntityTypeConfiguration<MccRevenueConcentrationDto>
{
    public void Configure(EntityTypeBuilder<MccRevenueConcentrationDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_mcc_revenue_concentration", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class MerchantRiskHotspotsViewConfiguration : IEntityTypeConfiguration<MerchantRiskHotspotDto>
{
    public void Configure(EntityTypeBuilder<MerchantRiskHotspotDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_merchant_risk_hotspots", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class CountryCrossBorderExposureViewConfiguration : IEntityTypeConfiguration<CountryCrossBorderExposureDto>
{
    public void Configure(EntityTypeBuilder<CountryCrossBorderExposureDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_country_cross_border_exposure", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ResponseCodeDeclineHealthViewConfiguration : IEntityTypeConfiguration<ResponseCodeDeclineHealthDto>
{
    public void Configure(EntityTypeBuilder<ResponseCodeDeclineHealthDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_response_code_decline_health", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class SettlementLagAnalysisViewConfiguration : IEntityTypeConfiguration<SettlementLagAnalysisDto>
{
    public void Configure(EntityTypeBuilder<SettlementLagAnalysisDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_settlement_lag_analysis", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class CurrencyFxDriftViewConfiguration : IEntityTypeConfiguration<CurrencyFxDriftDto>
{
    public void Configure(EntityTypeBuilder<CurrencyFxDriftDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_currency_fx_drift", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class InstallmentPortfolioSummaryViewConfiguration : IEntityTypeConfiguration<InstallmentPortfolioSummaryDto>
{
    public void Configure(EntityTypeBuilder<InstallmentPortfolioSummaryDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_installment_portfolio_summary", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class LoyaltyPointsEconomyViewConfiguration : IEntityTypeConfiguration<LoyaltyPointsEconomyDto>
{
    public void Configure(EntityTypeBuilder<LoyaltyPointsEconomyDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_loyalty_points_economy", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ClearingDisputeSummaryViewConfiguration : IEntityTypeConfiguration<ClearingDisputeSummaryDto>
{
    public void Configure(EntityTypeBuilder<ClearingDisputeSummaryDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_clearing_dispute_summary", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ClearingIoImbalanceViewConfiguration : IEntityTypeConfiguration<ClearingIoImbalanceDto>
{
    public void Configure(EntityTypeBuilder<ClearingIoImbalanceDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_clearing_io_imbalance", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class HighValueUnmatchedTransactionsViewConfiguration : IEntityTypeConfiguration<HighValueUnmatchedTransactionDto>
{
    public void Configure(EntityTypeBuilder<HighValueUnmatchedTransactionDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_high_value_unmatched_transactions", "reporting");
        builder.Property(x => x.DataScope).HasConversion<string>().HasColumnType("text");
    }
}

public class ReportingDocumentationViewConfiguration : IEntityTypeConfiguration<ReportingDocumentationDto>
{
    public void Configure(EntityTypeBuilder<ReportingDocumentationDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("rep_documentation", "reporting");
    }
}

