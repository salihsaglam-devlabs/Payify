using System.Reflection;
using LinkPara.ContextProvider;
using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Scheduler.API.Persistence;

public class SchedulerDbContext : BaseDbContext 
{
    private readonly IVaultClient _vaultClient;
    public DbSet<CronJob> CronJob { get; set; }

    public SchedulerDbContext(DbContextOptions options,
           IContextProvider contextProvider,
           IBus bus, IVaultClient vaultClient
           ) : base(options,contextProvider,null,bus)
    {
        _vaultClient = vaultClient;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return await base.SaveChangesAsync(cancellationToken);
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
        builder.Entity<CronJob>().ToTable("cron_job", schema);
    }
    
    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        var schema = "Core";
        builder.Entity<CronJob>().ToTable("CronJob", schema);
    }
}
