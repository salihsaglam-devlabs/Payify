using System.Reflection;
using LinkPara.ContextProvider;
using LinkPara.IKS.Domain.Entities;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.IKS.Infrastructure.Persistence;

public class IKSDbContext : BaseDbContext
{

    public DbSet<IKSTransaction> IKSTransaction { get; set; }
    public DbSet<IksTerminal> IksTerminal { get; set; }
    public DbSet<IksTerminalHistory> IksTerminalHistory { get; set; }
    private readonly IVaultClient _vaultClient;

    public IKSDbContext(DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService, IBus bus,
        IVaultClient vaultClient)
        : base(options, contextProvider, domainEventService, bus)
    {
        _vaultClient = vaultClient;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        var databaseProvider = _vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                CreateMsSqlMappings(builder);
                break;
            default:
                CreateDefaultMappings(builder);
                break;
        }

        base.OnModelCreating(builder);
    }


    private static void CreateDefaultMappings(ModelBuilder builder)
    {
        var schema = "core";
        builder.Entity<IKSTransaction>().ToTable("iks_transaction", schema);
        builder.Entity<TimeoutIKSTransaction>().ToTable("timeout_iks_transaction", schema);
        builder.Entity<IksTerminal>().ToTable("iks_terminal", schema);
        builder.Entity<IksTerminalHistory>().ToTable("iks_terminal_history", schema);
    }

    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        var schema = "Core";
        builder.Entity<IKSTransaction>().ToTable("IKSTransaction", schema);
        builder.Entity<TimeoutIKSTransaction>().ToTable("TimeoutIKSTransaction", schema);
        builder.Entity<IksTerminal>().ToTable("IksTerminal", schema);
        builder.Entity<IksTerminalHistory>().ToTable("IksTerminalHistory", schema);
    }

}