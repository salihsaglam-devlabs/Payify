using LinkPara.Billing.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Billing.Infrastructure.Persistence
{
    public class BillingDbContext : BaseDbContext
    {
        public DbSet<AuthorizationToken> AuthorizationToken { get; set; }
        public DbSet<Field> Field { get; set; }
        public DbSet<Institution> Institution { get; set; }
        public DbSet<InstitutionMapping> InstitutionMapping { get; set; }
        public DbSet<Sector> Sector { get; set; }
        public DbSet<SectorMapping> SectorMapping { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<Vendor> Vendor { get; set; }
        public DbSet<Commission> Commission { get; set; }
        public DbSet<SavedBill> SavedBill { get; set; }
        public DbSet<Summary> Summary { get; set; }
        public DbSet<InstitutionSummary> InstitutionSummary { get; set; }
        public DbSet<InstitutionDetail> InstitutionDetail { get; set; }
        public DbSet<TimeoutTransaction> TimeoutTransaction { get; set; }
        public DbSet<SynchronizationLog> SynchronizationLog { get; set; }
        public DbSet<TransactionReferenceCounter> TransactionReferenceCounter { get; set; }

        private readonly IVaultClient _vaultClient;
        public BillingDbContext(DbContextOptions options,
            IContextProvider contextProvider,
            IDomainEventService domainEventService,
            IBus bus,
            IVaultClient vaultClient)
            : base(options, contextProvider, domainEventService, bus)
        {
            _vaultClient = vaultClient;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return await base.SaveChangesAsync(cancellationToken);
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
            var schema = "core";
            var reconciliationSchema = "reconciliation";

            builder.Entity<AuthorizationToken>().ToTable("authorization_token", schema);
            builder.Entity<Field>().ToTable("field", schema);
            builder.Entity<Institution>().ToTable("institution", schema);
            builder.Entity<InstitutionMapping>().ToTable("institution_mapping", schema);
            builder.Entity<Sector>().ToTable("sector", schema);
            builder.Entity<SectorMapping>().ToTable("sector_mapping", schema);
            builder.Entity<Transaction>().ToTable("transaction", schema);
            builder.Entity<Vendor>().ToTable("vendor", schema);
            builder.Entity<Commission>().ToTable("commission", schema);
            builder.Entity<SavedBill>().ToTable("saved_bill", schema);
            builder.Entity<TimeoutTransaction>().ToTable("timeout_transaction", schema);
            builder.Entity<SynchronizationLog>().ToTable("synchronization_log", schema);
            builder.Entity<TransactionReferenceCounter>().ToTable("transaction_reference_counter", schema);

            builder.Entity<Summary>().ToTable("summary", reconciliationSchema);
            builder.Entity<InstitutionSummary>().ToTable("institution_summary", reconciliationSchema);
            builder.Entity<InstitutionDetail>().ToTable("institution_detail", reconciliationSchema);
        }

        private static void CreateMsSqlMappings(ModelBuilder builder)
        {
            var schema = "Core";
            var reconciliationSchema = "Reconciliation";

            builder.Entity<AuthorizationToken>().ToTable("AuthorizationToken", schema);
            builder.Entity<Field>().ToTable("Field", schema);
            builder.Entity<Institution>().ToTable("Institution", schema);
            builder.Entity<InstitutionMapping>().ToTable("InstitutionMapping", schema);
            builder.Entity<Sector>().ToTable("Sector", schema);
            builder.Entity<SectorMapping>().ToTable("SectorMapping", schema);
            builder.Entity<Transaction>().ToTable("Transaction", schema);
            builder.Entity<Vendor>().ToTable("Vendor", schema);
            builder.Entity<Commission>().ToTable("Commission", schema);
            builder.Entity<SavedBill>().ToTable("SavedBill", schema);
            builder.Entity<TimeoutTransaction>().ToTable("TimeoutTransaction", schema);
            builder.Entity<SynchronizationLog>().ToTable("SynchronizationLog", schema);
            builder.Entity<TransactionReferenceCounter>().ToTable("TransactionReferenceCounter", schema);

            builder.Entity<Summary>().ToTable("Summary", reconciliationSchema);
            builder.Entity<InstitutionSummary>().ToTable("InstitutionSummary", reconciliationSchema);
            builder.Entity<InstitutionDetail>().ToTable("InstitutionDetail", reconciliationSchema);
        }
    }
}