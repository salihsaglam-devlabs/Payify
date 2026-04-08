using LinkPara.Card.Domain.Entities;
using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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

    public DbSet<ReconciliationEvaluation> ReconciliationEvaluations { get; set; }
    public DbSet<ReconciliationOperation> ReconciliationOperations { get; set; }
    public DbSet<ReconciliationReview> ReconciliationReviews { get; set; }
    public DbSet<ReconciliationOperationExecution> ReconciliationOperationExecutions { get; set; }
    public DbSet<ReconciliationAlert> ReconciliationAlerts { get; set; }

    public DbSet<ArchiveBatch> ArchiveBatches { get; set; }
    public DbSet<ArchiveBatchItem> ArchiveBatchItems { get; set; }

    public DbSet<ArchiveIngestionFile> ArchiveIngestionFiles { get; set; }
    public DbSet<ArchiveIngestionFileLine> ArchiveIngestionFileLines { get; set; }
    public DbSet<ArchiveReconciliationEvaluation> ArchiveReconciliationEvaluations { get; set; }
    public DbSet<ArchiveReconciliationOperation> ArchiveReconciliationOperations { get; set; }
    public DbSet<ArchiveReconciliationReview> ArchiveReconciliationReviews { get; set; }
    public DbSet<ArchiveReconciliationOperationExecution> ArchiveReconciliationOperationExecutions { get; set; }
    public DbSet<ArchiveReconciliationAlert> ArchiveReconciliationAlerts { get; set; }

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

        builder.Entity<DebitAuthorization>().ToTable("debit_authorization", coreSchema);
        builder.Entity<DebitAuthorizationFee>().ToTable("debit_authorization_fee", coreSchema);
        builder.Entity<CustomerWalletCard>().ToTable("customer_wallet_card", coreSchema);
        
        builder.Entity<IngestionFile>().ToTable("file", ingestionSchema);
        builder.Entity<IngestionFileLine>().ToTable("file_line", ingestionSchema);

        builder.Entity<ReconciliationEvaluation>().ToTable("evaluation", reconSchema);
        builder.Entity<ReconciliationOperation>().ToTable("operation", reconSchema);
        builder.Entity<ReconciliationReview>().ToTable("review", reconSchema);
        builder.Entity<ReconciliationOperationExecution>().ToTable("operation_execution", reconSchema);
        builder.Entity<ReconciliationAlert>().ToTable("alert", reconSchema);

        builder.Entity<ArchiveBatch>().ToTable("archive_batch", archiveSchema);
        builder.Entity<ArchiveBatchItem>().ToTable("archive_batch_item", archiveSchema);

        builder.Entity<ArchiveIngestionFile>().ToTable("ingestion_file", archiveSchema);
        builder.Entity<ArchiveIngestionFileLine>().ToTable("ingestion_file_line", archiveSchema);
        builder.Entity<ArchiveReconciliationEvaluation>().ToTable("reconciliation_evaluation", archiveSchema);
        builder.Entity<ArchiveReconciliationOperation>().ToTable("reconciliation_operation", archiveSchema);
        builder.Entity<ArchiveReconciliationReview>().ToTable("reconciliation_review", archiveSchema);
        builder.Entity<ArchiveReconciliationOperationExecution>().ToTable("reconciliation_operation_execution", archiveSchema);
        builder.Entity<ArchiveReconciliationAlert>().ToTable("reconciliation_alert", archiveSchema);
    }

    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        const string coreSchema = "Core";
        const string ingestionSchema = "Ingestion";
        const string reconSchema = "Reconciliation";
        const string archiveSchema = "Archive";

        builder.Entity<DebitAuthorization>().ToTable("DebitAuthorization", coreSchema);
        builder.Entity<DebitAuthorizationFee>().ToTable("DebitAuthorizationFee", coreSchema);
        builder.Entity<CustomerWalletCard>().ToTable("CustomerWalletCard", coreSchema);
        
        builder.Entity<IngestionFile>().ToTable("File", ingestionSchema);
        builder.Entity<IngestionFileLine>().ToTable("FileLine", ingestionSchema);

        builder.Entity<ReconciliationEvaluation>().ToTable("Evaluation", reconSchema);
        builder.Entity<ReconciliationOperation>().ToTable("Operation", reconSchema);
        builder.Entity<ReconciliationReview>().ToTable("Review", reconSchema);
        builder.Entity<ReconciliationOperationExecution>().ToTable("OperationExecution", reconSchema);
        builder.Entity<ReconciliationAlert>().ToTable("Alert", reconSchema);

        builder.Entity<ArchiveBatch>().ToTable("archive_batch", archiveSchema);
        builder.Entity<ArchiveBatchItem>().ToTable("archive_batch_item", archiveSchema);

        builder.Entity<ArchiveIngestionFile>().ToTable("IngestionFile", archiveSchema);
        builder.Entity<ArchiveIngestionFileLine>().ToTable("IngestionFileLine", archiveSchema);
        builder.Entity<ArchiveReconciliationEvaluation>().ToTable("ReconciliationEvaluation", archiveSchema);
        builder.Entity<ArchiveReconciliationOperation>().ToTable("ReconciliationOperation", archiveSchema);
        builder.Entity<ArchiveReconciliationReview>().ToTable("ReconciliationReview", archiveSchema);
        builder.Entity<ArchiveReconciliationOperationExecution>().ToTable("ReconciliationOperationExecution", archiveSchema);
        builder.Entity<ArchiveReconciliationAlert>().ToTable("ReconciliationAlert", archiveSchema);
    }
}
