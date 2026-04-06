using LinkPara.ContextProvider;
using LinkPara.LogConsumers.Commons.Entities;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.LogConsumers.Persistence;

public class LogConsumerDbContext : BaseDbContext
{
    private readonly IVaultClient _vaultClient;
    public DbSet<AuditLog> AuditLog { get; set; }
    public DbSet<EntityChangeLog> EntityChangeLog { get; set; }

    public LogConsumerDbContext(DbContextOptions options,
          IContextProvider contextProvider,
          IBus bus, IVaultClient vaultClient) : base(options, contextProvider, null,bus)
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
        builder.Entity<AuditLog>().ToTable("audit_log", schema);
        builder.Entity<EntityChangeLog>().ToTable("entity_change_log", schema);
    }
    
    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        var schema = "Core";
        builder.Entity<AuditLog>().ToTable("AuditLog", schema);
        builder.Entity<EntityChangeLog>().ToTable("EntityChangeLog", schema);
    }
}
