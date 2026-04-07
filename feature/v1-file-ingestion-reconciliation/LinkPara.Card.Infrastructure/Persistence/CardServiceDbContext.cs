using LinkPara.ContextProvider;
using LinkPara.Card.Domain.Entities;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
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
    private readonly IVaultClient _vaultClient;
    public CardDbContext(DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService,
        IBus bus, IConfiguration configuration, IVaultClient vaultClient)
        : base(options, contextProvider, domainEventService, bus)
    {
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

    public DbSet<ImportedFile> ImportedFiles => Set<ImportedFile>();
    public DbSet<ImportedFileRow> ImportedFileRows => Set<ImportedFileRow>();
    public DbSet<CardTransactionRecord> CardTransactionRecords => Set<CardTransactionRecord>();
    public DbSet<DebitAuthorization> DebitAuthorizations => Set<DebitAuthorization>();
    public DbSet<CustomerWalletCard> CustomerWalletCards => Set<CustomerWalletCard>();
    public DbSet<ClearingRecord> ClearingRecords => Set<ClearingRecord>();
    public DbSet<ReconciliationOperation> ReconciliationOperations => Set<ReconciliationOperation>();
    public DbSet<ReconciliationOperationExecution> ReconciliationOperationExecutions => Set<ReconciliationOperationExecution>();
    public DbSet<ReconciliationManualReviewItem> ReconciliationManualReviewItems => Set<ReconciliationManualReviewItem>();
    public DbSet<ReconciliationEvaluation> ReconciliationEvaluations => Set<ReconciliationEvaluation>();
    public DbSet<ProcessExecutionLock> ProcessExecutionLocks => Set<ProcessExecutionLock>();

    private static void CreateDefaultMappings(ModelBuilder builder)
    {
        const string coreSchema = "core";
        builder.Entity<ImportedFile>().ToTable("imported_files", coreSchema);
        builder.Entity<ImportedFileRow>().ToTable("imported_file_rows", coreSchema);
        builder.Entity<CardTransactionRecord>().ToTable("card_transaction_records", coreSchema);
        builder.Entity<DebitAuthorization>().ToTable("debit_authorization", coreSchema);
        builder.Entity<CustomerWalletCard>().ToTable("customer_wallet_card", coreSchema);
        builder.Entity<ClearingRecord>().ToTable("clearing_records", coreSchema);
        builder.Entity<ReconciliationOperation>().ToTable("reconciliation_operations", coreSchema);
        builder.Entity<ReconciliationOperationExecution>().ToTable("reconciliation_operation_executions", coreSchema);
        builder.Entity<ReconciliationManualReviewItem>().ToTable("reconciliation_manual_review_items", coreSchema);
        builder.Entity<ReconciliationEvaluation>().ToTable("reconciliation_evaluations", coreSchema);
        builder.Entity<ProcessExecutionLock>().ToTable("process_execution_lock", coreSchema);
    }

    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        const string coreSchema = "Core";
        builder.Entity<ImportedFile>().ToTable("ImportedFiles", coreSchema);
        builder.Entity<ImportedFileRow>().ToTable("ImportedFileRows", coreSchema);
        builder.Entity<CardTransactionRecord>().ToTable("CardTransactionRecords", coreSchema);
        builder.Entity<DebitAuthorization>().ToTable("DebitAuthorization", coreSchema);
        builder.Entity<CustomerWalletCard>().ToTable("CustomerWalletCard", coreSchema);
        builder.Entity<ClearingRecord>().ToTable("ClearingRecords", coreSchema);
        builder.Entity<ReconciliationOperation>().ToTable("ReconciliationOperations", coreSchema);
        builder.Entity<ReconciliationOperationExecution>().ToTable("ReconciliationOperationExecutions", coreSchema);
        builder.Entity<ReconciliationManualReviewItem>().ToTable("ReconciliationManualReviewItems", coreSchema);
        builder.Entity<ReconciliationEvaluation>().ToTable("ReconciliationEvaluations", coreSchema);
        builder.Entity<ProcessExecutionLock>().ToTable("ProcessExecutionLock", coreSchema);
    }

}
