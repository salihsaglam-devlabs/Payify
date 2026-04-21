using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using LinkPara.Card.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using LinkPara.Card.Domain.Entities.Archive.Persistence;
using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;

namespace LinkPara.Card.Infrastructure.Persistence;

public class CardDbContext : BaseDbContext
{
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;

    public DbSet<CustomerWalletCard> CustomerWalletCard { get; set; }
    public DbSet<DebitAuthorization> DebitAuthorization { get; set; } 
    public DbSet<DebitAuthorizationFee> DebitAuthorizationFee { get; set; } 
    
    public DbSet<IngestionFile> IngestionFiles { get; set; }
    public DbSet<IngestionFileLine> IngestionFileLines { get; set; }

    public DbSet<IngestionCardVisaDetail> IngestionCardVisaDetails { get; set; }
    public DbSet<IngestionCardMscDetail> IngestionCardMscDetails { get; set; }
    public DbSet<IngestionCardBkmDetail> IngestionCardBkmDetails { get; set; }
    public DbSet<IngestionClearingVisaDetail> IngestionClearingVisaDetails { get; set; }
    public DbSet<IngestionClearingMscDetail> IngestionClearingMscDetails { get; set; }
    public DbSet<IngestionClearingBkmDetail> IngestionClearingBkmDetails { get; set; }

    public DbSet<ReconciliationEvaluation> ReconciliationEvaluations { get; set; }
    public DbSet<ReconciliationOperation> ReconciliationOperations { get; set; }
    public DbSet<ReconciliationReview> ReconciliationReviews { get; set; }
    public DbSet<ReconciliationOperationExecution> ReconciliationOperationExecutions { get; set; }
    public DbSet<ReconciliationAlert> ReconciliationAlerts { get; set; }

    public DbSet<ArchiveLog> ArchiveLogs { get; set; }

    public DbSet<ArchiveIngestionFile> ArchiveIngestionFiles { get; set; }
    public DbSet<ArchiveIngestionFileLine> ArchiveIngestionFileLines { get; set; }

    public DbSet<ArchiveIngestionCardVisaDetail> ArchiveIngestionCardVisaDetails { get; set; }
    public DbSet<ArchiveIngestionCardMscDetail> ArchiveIngestionCardMscDetails { get; set; }
    public DbSet<ArchiveIngestionCardBkmDetail> ArchiveIngestionCardBkmDetails { get; set; }
    public DbSet<ArchiveIngestionClearingVisaDetail> ArchiveIngestionClearingVisaDetails { get; set; }
    public DbSet<ArchiveIngestionClearingMscDetail> ArchiveIngestionClearingMscDetails { get; set; }
    public DbSet<ArchiveIngestionClearingBkmDetail> ArchiveIngestionClearingBkmDetails { get; set; }
    public DbSet<ArchiveReconciliationEvaluation> ArchiveReconciliationEvaluations { get; set; }
    public DbSet<ArchiveReconciliationOperation> ArchiveReconciliationOperations { get; set; }
    public DbSet<ArchiveReconciliationReview> ArchiveReconciliationReviews { get; set; }
    public DbSet<ArchiveReconciliationOperationExecution> ArchiveReconciliationOperationExecutions { get; set; }
    public DbSet<ArchiveReconciliationAlert> ArchiveReconciliationAlerts { get; set; }
    
    public DbSet<DailyTransactionVolumeDto> DailyTransactionVolume { get; set; }
    public DbSet<MccRevenueConcentrationDto> MccRevenueConcentration { get; set; }
    public DbSet<MerchantRiskHotspotDto> MerchantRiskHotspots { get; set; }
    public DbSet<CountryCrossBorderExposureDto> CountryCrossBorderExposure { get; set; }
    public DbSet<ResponseCodeDeclineHealthDto> ResponseCodeDeclineHealth { get; set; }
    public DbSet<SettlementLagAnalysisDto> SettlementLagAnalysis { get; set; }
    public DbSet<CurrencyFxDriftDto> CurrencyFxDrift { get; set; }
    public DbSet<InstallmentPortfolioSummaryDto> InstallmentPortfolioSummary { get; set; }
    public DbSet<LoyaltyPointsEconomyDto> LoyaltyPointsEconomy { get; set; }
    public DbSet<ClearingDisputeSummaryDto> ClearingDisputeSummary { get; set; }
    public DbSet<ClearingIoImbalanceDto> ClearingIoImbalance { get; set; }
    public DbSet<HighValueUnmatchedTransactionDto> HighValueUnmatchedTransactions { get; set; }
    
    public DbSet<ActionRadarDto> ActionRadar { get; set; }
    public DbSet<UnhealthyFileDto> UnhealthyFiles { get; set; }
    public DbSet<StuckPipelineItemDto> StuckPipelineItems { get; set; }
    public DbSet<ReconFailureCategorizationDto> ReconFailureCategorization { get; set; }
    public DbSet<ManualReviewPressureDto> ManualReviewPressure { get; set; }
    public DbSet<AlertDeliveryHealthDto> AlertDeliveryHealth { get; set; }
    public DbSet<UnmatchedFinancialExposureDto> UnmatchedFinancialExposure { get; set; }
    public DbSet<CardClearingImbalanceDto> CardClearingImbalance { get; set; }
    public DbSet<ReconciliationQualityScoreDto> ReconciliationQualityScore { get; set; }
    public DbSet<MisleadingSuccessCaseDto> MisleadingSuccessCases { get; set; }
    public DbSet<ArchivePipelineHealthDto> ArchivePipelineHealth { get; set; }
    public DbSet<ReportingDocumentationDto> ReportingDocumentation { get; set; }

    public CardDbContext(DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService,
        IBus bus, IConfiguration configuration, IVaultClient vaultClient)
        : base(options, contextProvider, domainEventService, bus)
    {
        _configuration = configuration;
        _vaultClient = vaultClient;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        var databaseProvider = _vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");
        switch (databaseProvider)
        {
            case "MsSql":
                CreateMsSqlMappings(modelBuilder);
                break;
            default:
                CreateDefaultMappings(modelBuilder);
                break;
        }
        base.OnModelCreating(modelBuilder);
    }
 

    private static void CreateDefaultMappings(ModelBuilder builder)
    {
        const string coreSchema = "core";
        const string ingestionSchema = "ingestion";
        const string reconSchema = "reconciliation";
        const string archiveSchema = "archive";
        const string reportingSchema = "reporting";

        builder.Entity<DebitAuthorization>().ToTable("debit_authorization", coreSchema);
        builder.Entity<DebitAuthorizationFee>().ToTable("debit_authorization_fee", coreSchema);
        builder.Entity<CustomerWalletCard>().ToTable("customer_wallet_card", coreSchema);
        
        builder.Entity<IngestionFile>().ToTable("file", ingestionSchema);
        builder.Entity<IngestionFileLine>().ToTable("file_line", ingestionSchema);

        builder.Entity<IngestionCardVisaDetail>().ToTable("card_visa_detail", ingestionSchema);
        builder.Entity<IngestionCardMscDetail>().ToTable("card_msc_detail", ingestionSchema);
        builder.Entity<IngestionCardBkmDetail>().ToTable("card_bkm_detail", ingestionSchema);
        builder.Entity<IngestionClearingVisaDetail>().ToTable("clearing_visa_detail", ingestionSchema);
        builder.Entity<IngestionClearingMscDetail>().ToTable("clearing_msc_detail", ingestionSchema);
        builder.Entity<IngestionClearingBkmDetail>().ToTable("clearing_bkm_detail", ingestionSchema);

        builder.Entity<ReconciliationEvaluation>().ToTable("evaluation", reconSchema);
        builder.Entity<ReconciliationOperation>().ToTable("operation", reconSchema);
        builder.Entity<ReconciliationReview>().ToTable("review", reconSchema);
        builder.Entity<ReconciliationOperationExecution>().ToTable("operation_execution", reconSchema);
        builder.Entity<ReconciliationAlert>().ToTable("alert", reconSchema);

        builder.Entity<ArchiveLog>().ToTable("archive_log", archiveSchema);

        builder.Entity<ArchiveIngestionFile>().ToTable("ingestion_file", archiveSchema);
        builder.Entity<ArchiveIngestionFileLine>().ToTable("ingestion_file_line", archiveSchema);

        builder.Entity<ArchiveIngestionCardVisaDetail>().ToTable("ingestion_card_visa_detail", archiveSchema);
        builder.Entity<ArchiveIngestionCardMscDetail>().ToTable("ingestion_card_msc_detail", archiveSchema);
        builder.Entity<ArchiveIngestionCardBkmDetail>().ToTable("ingestion_card_bkm_detail", archiveSchema);
        builder.Entity<ArchiveIngestionClearingVisaDetail>().ToTable("ingestion_clearing_visa_detail", archiveSchema);
        builder.Entity<ArchiveIngestionClearingMscDetail>().ToTable("ingestion_clearing_msc_detail", archiveSchema);
        builder.Entity<ArchiveIngestionClearingBkmDetail>().ToTable("ingestion_clearing_bkm_detail", archiveSchema);

        builder.Entity<ArchiveReconciliationEvaluation>().ToTable("reconciliation_evaluation", archiveSchema);
        builder.Entity<ArchiveReconciliationOperation>().ToTable("reconciliation_operation", archiveSchema);
        builder.Entity<ArchiveReconciliationReview>().ToTable("reconciliation_review", archiveSchema);
        builder.Entity<ArchiveReconciliationOperationExecution>().ToTable("reconciliation_operation_execution", archiveSchema);
        builder.Entity<ArchiveReconciliationAlert>().ToTable("reconciliation_alert", archiveSchema);

        builder.Entity<DailyTransactionVolumeDto>().ToView("rep_daily_transaction_volume", reportingSchema);
        builder.Entity<MccRevenueConcentrationDto>().ToView("rep_mcc_revenue_concentration", reportingSchema);
        builder.Entity<MerchantRiskHotspotDto>().ToView("rep_merchant_risk_hotspots", reportingSchema);
        builder.Entity<CountryCrossBorderExposureDto>().ToView("rep_country_cross_border_exposure", reportingSchema);
        builder.Entity<ResponseCodeDeclineHealthDto>().ToView("rep_response_code_decline_health", reportingSchema);
        builder.Entity<SettlementLagAnalysisDto>().ToView("rep_settlement_lag_analysis", reportingSchema);
        builder.Entity<CurrencyFxDriftDto>().ToView("rep_currency_fx_drift", reportingSchema);
        builder.Entity<InstallmentPortfolioSummaryDto>().ToView("rep_installment_portfolio_summary", reportingSchema);
        builder.Entity<LoyaltyPointsEconomyDto>().ToView("rep_loyalty_points_economy", reportingSchema);
        builder.Entity<ClearingDisputeSummaryDto>().ToView("rep_clearing_dispute_summary", reportingSchema);
        builder.Entity<ClearingIoImbalanceDto>().ToView("rep_clearing_io_imbalance", reportingSchema);
        builder.Entity<HighValueUnmatchedTransactionDto>().ToView("rep_high_value_unmatched_transactions", reportingSchema);
        builder.Entity<ActionRadarDto>().ToView("rep_action_radar", reportingSchema);
        builder.Entity<UnhealthyFileDto>().ToView("rep_unhealthy_files", reportingSchema);
        builder.Entity<StuckPipelineItemDto>().ToView("rep_stuck_pipeline_items", reportingSchema);
        builder.Entity<ReconFailureCategorizationDto>().ToView("rep_recon_failure_categorization", reportingSchema);
        builder.Entity<ManualReviewPressureDto>().ToView("rep_manual_review_pressure", reportingSchema);
        builder.Entity<AlertDeliveryHealthDto>().ToView("rep_alert_delivery_health", reportingSchema);
        builder.Entity<UnmatchedFinancialExposureDto>().ToView("rep_unmatched_financial_exposure", reportingSchema);
        builder.Entity<CardClearingImbalanceDto>().ToView("rep_card_clearing_imbalance", reportingSchema);
        builder.Entity<ReconciliationQualityScoreDto>().ToView("rep_reconciliation_quality_score", reportingSchema);
        builder.Entity<MisleadingSuccessCaseDto>().ToView("rep_misleading_success_cases", reportingSchema);
        builder.Entity<ArchivePipelineHealthDto>().ToView("rep_archive_pipeline_health", reportingSchema);
        builder.Entity<ReportingDocumentationDto>().ToView("rep_documentation", reportingSchema);
    }

    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        const string coreSchema = "Core";
        const string ingestionSchema = "Ingestion";
        const string reconSchema = "Reconciliation";
        const string archiveSchema = "Archive";
        const string reportingSchema = "Reporting";

        builder.Entity<DebitAuthorization>().ToTable("DebitAuthorization", coreSchema);
        builder.Entity<DebitAuthorizationFee>().ToTable("DebitAuthorizationFee", coreSchema);
        builder.Entity<CustomerWalletCard>().ToTable("CustomerWalletCard", coreSchema);
        
        builder.Entity<IngestionFile>().ToTable("File", ingestionSchema);
        builder.Entity<IngestionFileLine>().ToTable("FileLine", ingestionSchema);

        builder.Entity<IngestionCardVisaDetail>().ToTable("CardVisaDetail", ingestionSchema);
        builder.Entity<IngestionCardMscDetail>().ToTable("CardMscDetail", ingestionSchema);
        builder.Entity<IngestionCardBkmDetail>().ToTable("CardBkmDetail", ingestionSchema);
        builder.Entity<IngestionClearingVisaDetail>().ToTable("ClearingVisaDetail", ingestionSchema);
        builder.Entity<IngestionClearingMscDetail>().ToTable("ClearingMscDetail", ingestionSchema);
        builder.Entity<IngestionClearingBkmDetail>().ToTable("ClearingBkmDetail", ingestionSchema);

        builder.Entity<ReconciliationEvaluation>().ToTable("Evaluation", reconSchema);
        builder.Entity<ReconciliationOperation>().ToTable("Operation", reconSchema);
        builder.Entity<ReconciliationReview>().ToTable("Review", reconSchema);
        builder.Entity<ReconciliationOperationExecution>().ToTable("OperationExecution", reconSchema);
        builder.Entity<ReconciliationAlert>().ToTable("Alert", reconSchema);

        builder.Entity<ArchiveLog>().ToTable("ArchiveLog", archiveSchema);

        builder.Entity<ArchiveIngestionFile>().ToTable("IngestionFile", archiveSchema);
        builder.Entity<ArchiveIngestionFileLine>().ToTable("IngestionFileLine", archiveSchema);

        builder.Entity<ArchiveIngestionCardVisaDetail>().ToTable("IngestionCardVisaDetail", archiveSchema);
        builder.Entity<ArchiveIngestionCardMscDetail>().ToTable("IngestionCardMscDetail", archiveSchema);
        builder.Entity<ArchiveIngestionCardBkmDetail>().ToTable("IngestionCardBkmDetail", archiveSchema);
        builder.Entity<ArchiveIngestionClearingVisaDetail>().ToTable("IngestionClearingVisaDetail", archiveSchema);
        builder.Entity<ArchiveIngestionClearingMscDetail>().ToTable("IngestionClearingMscDetail", archiveSchema);
        builder.Entity<ArchiveIngestionClearingBkmDetail>().ToTable("IngestionClearingBkmDetail", archiveSchema);

        builder.Entity<ArchiveReconciliationEvaluation>().ToTable("ReconciliationEvaluation", archiveSchema);
        builder.Entity<ArchiveReconciliationOperation>().ToTable("ReconciliationOperation", archiveSchema);
        builder.Entity<ArchiveReconciliationReview>().ToTable("ReconciliationReview", archiveSchema);
        builder.Entity<ArchiveReconciliationOperationExecution>().ToTable("ReconciliationOperationExecution", archiveSchema);
        builder.Entity<ArchiveReconciliationAlert>().ToTable("ReconciliationAlert", archiveSchema);
        
        builder.Entity<DailyTransactionVolumeDto>().ToView("VwDailyTransactionVolume", reportingSchema);
        builder.Entity<MccRevenueConcentrationDto>().ToView("VwMccRevenueConcentration", reportingSchema);
        builder.Entity<MerchantRiskHotspotDto>().ToView("VwMerchantRiskHotspots", reportingSchema);
        builder.Entity<CountryCrossBorderExposureDto>().ToView("VwCountryCrossBorderExposure", reportingSchema);
        builder.Entity<ResponseCodeDeclineHealthDto>().ToView("VwResponseCodeDeclineHealth", reportingSchema);
        builder.Entity<SettlementLagAnalysisDto>().ToView("VwSettlementLagAnalysis", reportingSchema);
        builder.Entity<CurrencyFxDriftDto>().ToView("VwCurrencyFxDrift", reportingSchema);
        builder.Entity<InstallmentPortfolioSummaryDto>().ToView("VwInstallmentPortfolioSummary", reportingSchema);
        builder.Entity<LoyaltyPointsEconomyDto>().ToView("VwLoyaltyPointsEconomy", reportingSchema);
        builder.Entity<ClearingDisputeSummaryDto>().ToView("VwClearingDisputeSummary", reportingSchema);
        builder.Entity<ClearingIoImbalanceDto>().ToView("VwClearingIoImbalance", reportingSchema);
        builder.Entity<HighValueUnmatchedTransactionDto>().ToView("VwHighValueUnmatchedTransactions", reportingSchema);
        builder.Entity<ActionRadarDto>().ToView("VwActionRadar", reportingSchema);
        builder.Entity<UnhealthyFileDto>().ToView("VwUnhealthyFiles", reportingSchema);
        builder.Entity<StuckPipelineItemDto>().ToView("VwStuckPipelineItems", reportingSchema);
        builder.Entity<ReconFailureCategorizationDto>().ToView("VwReconFailureCategorization", reportingSchema);
        builder.Entity<ManualReviewPressureDto>().ToView("VwManualReviewPressure", reportingSchema);
        builder.Entity<AlertDeliveryHealthDto>().ToView("VwAlertDeliveryHealth", reportingSchema);
        builder.Entity<UnmatchedFinancialExposureDto>().ToView("VwUnmatchedFinancialExposure", reportingSchema);
        builder.Entity<CardClearingImbalanceDto>().ToView("VwCardClearingImbalance", reportingSchema);
        builder.Entity<ReconciliationQualityScoreDto>().ToView("VwReconciliationQualityScore", reportingSchema);
        builder.Entity<MisleadingSuccessCaseDto>().ToView("VwMisleadingSuccessCases", reportingSchema);
        builder.Entity<ArchivePipelineHealthDto>().ToView("VwArchivePipelineHealth", reportingSchema);
        builder.Entity<ReportingDocumentationDto>().ToView("VwReportingDocumentation", reportingSchema);
    }
}

