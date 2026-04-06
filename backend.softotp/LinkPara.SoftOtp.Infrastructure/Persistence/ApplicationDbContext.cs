using System.Reflection;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.SoftOtp.Infrastructure.Persistence;

public class ApplicationDbContext : BaseDbContext
{
    private readonly IVaultClient _vaultClient;
    
    public ApplicationDbContext(
        DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService,
        IBus bus, 
        IVaultClient vaultClient
    ) : base(options, contextProvider, domainEventService, bus)
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
    }
    
    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        var schema = "Core";
    }
}