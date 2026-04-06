using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MassTransit;
using LinkPara.Fraud.Domain.Entities;
using Microsoft.Extensions.Configuration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Fraud.Infrastructure.Persistence;

public class FraudDbContext : BaseDbContext
{
    public DbSet<TransactionMonitoring> TransactionMonitoring { get; set; }
    public DbSet<IntegrationLog> IntegrationLog { get; set; }
    public DbSet<SearchLog> SearchLog { get; set; }
    public DbSet<TriggeredRule> TriggeredRule { get; set; }
    public DbSet<TriggeredRuleSetKey> TriggeredRuleSetKey { get; set; }
    public DbSet<OngoingMonitoring> OngoingMonitoring { get; set; }

    private readonly IVaultClient _vaultClient;

    public FraudDbContext(DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService,
        IBus bus,
        IVaultClient vaultClient)
        : base(options, contextProvider, domainEventService,bus)
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

    private static void CreateDefaultMappings(ModelBuilder builder)
    {
        var schema = "core";
        builder.Entity<TriggeredRuleSetKey>().ToTable("triggered_rule_set_key", schema);
        builder.Entity<TransactionMonitoring>().ToTable("transaction_monitoring", schema);
        builder.Entity<IntegrationLog>().ToTable("integration_log", schema);
        builder.Entity<SearchLog>().ToTable("search_log", schema);
        builder.Entity<TriggeredRule>().ToTable("triggered_rule", schema);
        builder.Entity<OngoingMonitoring>().ToTable("ongoing_monitoring", schema);
    }

    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        var schema = "Core";
        builder.Entity<TriggeredRuleSetKey>().ToTable("TriggeredRuleSetKey", schema);
        builder.Entity<TransactionMonitoring>().ToTable("TransactionMonitoring", schema);
        builder.Entity<IntegrationLog>().ToTable("IntegrationLog", schema);
        builder.Entity<SearchLog>().ToTable("SearchLog", schema);
        builder.Entity<TriggeredRule>().ToTable("TriggeredRule", schema);
        builder.Entity<OngoingMonitoring>().ToTable("OngoingMonitoring", schema);
    }

}