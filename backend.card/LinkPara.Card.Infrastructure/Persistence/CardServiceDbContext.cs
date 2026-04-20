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
    
    public DbSet<IngestionFileOverviewDto> IngestionFileOverview { get; set; }
    public DbSet<IngestionFileQualityDto> IngestionFileQuality { get; set; }
    public DbSet<IngestionDailySummaryDto> IngestionDailySummary { get; set; }
    public DbSet<IngestionNetworkMatrixDto> IngestionNetworkMatrix { get; set; }
    public DbSet<IngestionExceptionHotspotDto> IngestionExceptionHotspot { get; set; }
    public DbSet<ReconDailyOverviewDto> ReconDailyOverview { get; set; }
    public DbSet<ReconOpenItemDto> ReconOpenItem { get; set; }
    public DbSet<ReconOpenItemAgingDto> ReconOpenItemAging { get; set; }
    public DbSet<ReconManualReviewQueueDto> ReconManualReviewQueue { get; set; }
    public DbSet<ReconAlertSummaryDto> ReconAlertSummary { get; set; }
    public DbSet<ReconCardContentDailyDto> ReconCardContentDaily { get; set; }
    public DbSet<ReconClearingContentDailyDto> ReconClearingContentDaily { get; set; }
    public DbSet<ReconContentDailyDto> ReconContentDaily { get; set; }
    public DbSet<ReconClearingControlStatAnalysisDto> ReconClearingControlStatAnalysis { get; set; }
    public DbSet<ReconFinancialSummaryDto> ReconFinancialSummary { get; set; }
    public DbSet<ReconResponseStatusAnalysisDto> ReconResponseStatusAnalysis { get; set; }
    public DbSet<ArchiveRunOverviewDto> ArchiveRunOverview { get; set; }
    public DbSet<ArchiveEligibilityDto> ArchiveEligibility { get; set; }
    public DbSet<ArchiveBacklogTrendDto> ArchiveBacklogTrend { get; set; }
    public DbSet<ArchiveRetentionSnapshotDto> ArchiveRetentionSnapshot { get; set; }
    public DbSet<FileReconSummaryDto> FileReconSummary { get; set; }
    public DbSet<ReconMatchRateTrendDto> ReconMatchRateTrend { get; set; }
    public DbSet<ReconGapAnalysisDto> ReconGapAnalysis { get; set; }
    public DbSet<UnmatchedTransactionAgingDto> UnmatchedTransactionAging { get; set; }
    public DbSet<NetworkReconScorecardDto> NetworkReconScorecard { get; set; }
    public DbSet<ReconCardContentDailyDto> ReconArchiveCardContentDaily { get; set; }
    public DbSet<ReconClearingContentDailyDto> ReconArchiveClearingContentDaily { get; set; }
    public DbSet<CardClearingCorrelationDto> CardClearingCorrelation { get; set; }

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
        
        builder.Entity<IngestionFileOverviewDto>().ToView("vw_ingestion_file_overview", reportingSchema).HasNoKey();
        builder.Entity<IngestionFileQualityDto>().ToView("vw_ingestion_file_quality", reportingSchema).HasNoKey();
        builder.Entity<IngestionDailySummaryDto>().ToView("vw_ingestion_daily_summary", reportingSchema).HasNoKey();
        builder.Entity<IngestionNetworkMatrixDto>().ToView("vw_ingestion_network_matrix", reportingSchema).HasNoKey();
        builder.Entity<IngestionExceptionHotspotDto>().ToView("vw_ingestion_exception_hotspots", reportingSchema).HasNoKey();
        builder.Entity<ReconDailyOverviewDto>().ToView("vw_recon_daily_overview", reportingSchema).HasNoKey();
        builder.Entity<ReconOpenItemDto>().ToView("vw_recon_open_items", reportingSchema).HasNoKey();
        builder.Entity<ReconOpenItemAgingDto>().ToView("vw_recon_open_item_aging", reportingSchema).HasNoKey();
        builder.Entity<ReconManualReviewQueueDto>().ToView("vw_recon_manual_review_queue", reportingSchema).HasNoKey();
        builder.Entity<ReconAlertSummaryDto>().ToView("vw_recon_alert_summary", reportingSchema).HasNoKey();
        builder.Entity<ReconCardContentDailyDto>().ToView("vw_recon_live_card_content_daily", reportingSchema).HasNoKey();
        builder.Entity<ReconClearingContentDailyDto>().ToView("vw_recon_live_clearing_content_daily", reportingSchema).HasNoKey();
        builder.Entity<ReconContentDailyDto>().ToView("vw_recon_content_daily", reportingSchema).HasNoKey();
        builder.Entity<ReconClearingControlStatAnalysisDto>().ToView("vw_recon_clearing_controlstat_analysis", reportingSchema).HasNoKey();
        builder.Entity<ReconFinancialSummaryDto>().ToView("vw_recon_financial_summary", reportingSchema).HasNoKey();
        builder.Entity<ReconResponseStatusAnalysisDto>().ToView("vw_recon_response_status_analysis", reportingSchema).HasNoKey();
        builder.Entity<ArchiveRunOverviewDto>().ToView("vw_archive_run_overview", reportingSchema).HasNoKey();
        builder.Entity<ArchiveEligibilityDto>().ToView("vw_archive_eligibility", reportingSchema).HasNoKey();
        builder.Entity<ArchiveBacklogTrendDto>().ToView("vw_archive_backlog_trend", reportingSchema).HasNoKey();
        builder.Entity<ArchiveRetentionSnapshotDto>().ToView("vw_archive_retention_snapshot", reportingSchema).HasNoKey();
        builder.Entity<FileReconSummaryDto>().ToView("vw_file_recon_summary", reportingSchema).HasNoKey();
        builder.Entity<ReconMatchRateTrendDto>().ToView("vw_recon_match_rate_trend", reportingSchema).HasNoKey();
        builder.Entity<ReconGapAnalysisDto>().ToView("vw_recon_gap_analysis", reportingSchema).HasNoKey();
        builder.Entity<UnmatchedTransactionAgingDto>().ToView("vw_unmatched_transaction_aging", reportingSchema).HasNoKey();
        builder.Entity<NetworkReconScorecardDto>().ToView("vw_network_recon_scorecard", reportingSchema).HasNoKey();
        builder.Entity<CardClearingCorrelationDto>().ToView("vw_card_clearing_correlation", reportingSchema).HasNoKey();
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
        
        builder.Entity<IngestionFileOverviewDto>().ToView("VwIngestionFileOverview", reportingSchema).HasNoKey();
        builder.Entity<IngestionFileQualityDto>().ToView("VwIngestionFileQuality", reportingSchema).HasNoKey();
        builder.Entity<IngestionDailySummaryDto>().ToView("VwIngestionDailySummary", reportingSchema).HasNoKey();
        builder.Entity<IngestionNetworkMatrixDto>().ToView("VwIngestionNetworkMatrix", reportingSchema).HasNoKey();
        builder.Entity<IngestionExceptionHotspotDto>().ToView("VwIngestionExceptionHotspots", reportingSchema).HasNoKey();
        builder.Entity<ReconDailyOverviewDto>().ToView("VwReconDailyOverview", reportingSchema).HasNoKey();
        builder.Entity<ReconOpenItemDto>().ToView("VwReconOpenItems", reportingSchema).HasNoKey();
        builder.Entity<ReconOpenItemAgingDto>().ToView("VwReconOpenItemAging", reportingSchema).HasNoKey();
        builder.Entity<ReconManualReviewQueueDto>().ToView("VwReconManualReviewQueue", reportingSchema).HasNoKey();
        builder.Entity<ReconAlertSummaryDto>().ToView("VwReconAlertSummary", reportingSchema).HasNoKey();
        builder.Entity<ReconCardContentDailyDto>().ToView("VwReconLiveCardContentDaily", reportingSchema).HasNoKey();
        builder.Entity<ReconClearingContentDailyDto>().ToView("VwReconLiveClearingContentDaily", reportingSchema).HasNoKey();
        builder.Entity<ReconContentDailyDto>().ToView("VwReconContentDaily", reportingSchema).HasNoKey();
        builder.Entity<ReconClearingControlStatAnalysisDto>().ToView("VwReconClearingControlstatAnalysis", reportingSchema).HasNoKey();
        builder.Entity<ReconFinancialSummaryDto>().ToView("VwReconFinancialSummary", reportingSchema).HasNoKey();
        builder.Entity<ReconResponseStatusAnalysisDto>().ToView("VwReconResponseStatusAnalysis", reportingSchema).HasNoKey();
        builder.Entity<ArchiveRunOverviewDto>().ToView("VwArchiveRunOverview", reportingSchema).HasNoKey();
        builder.Entity<ArchiveEligibilityDto>().ToView("VwArchiveEligibility", reportingSchema).HasNoKey();
        builder.Entity<ArchiveBacklogTrendDto>().ToView("VwArchiveBacklogTrend", reportingSchema).HasNoKey();
        builder.Entity<ArchiveRetentionSnapshotDto>().ToView("VwArchiveRetentionSnapshot", reportingSchema).HasNoKey();
        builder.Entity<FileReconSummaryDto>().ToView("VwFileReconSummary", reportingSchema).HasNoKey();
        builder.Entity<ReconMatchRateTrendDto>().ToView("VwReconMatchRateTrend", reportingSchema).HasNoKey();
        builder.Entity<ReconGapAnalysisDto>().ToView("VwReconGapAnalysis", reportingSchema).HasNoKey();
        builder.Entity<UnmatchedTransactionAgingDto>().ToView("VwUnmatchedTransactionAging", reportingSchema).HasNoKey();
        builder.Entity<NetworkReconScorecardDto>().ToView("VwNetworkReconScorecard", reportingSchema).HasNoKey();
        builder.Entity<CardClearingCorrelationDto>().ToView("VwCardClearingCorrelation", reportingSchema).HasNoKey();
    }
}

