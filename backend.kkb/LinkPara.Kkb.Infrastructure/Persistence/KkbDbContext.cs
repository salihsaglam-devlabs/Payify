using Microsoft.EntityFrameworkCore;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using LinkPara.Kkb.Domain.Entities;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Kkb.Infrastructure.Persistence;

public class KkbDbContext : BaseDbContext
{
    private readonly IVaultClient _vaultClient;

    public DbSet<KkbAuthorizationToken> KkbAuthorizationToken { get; set; }

    public KkbDbContext(DbContextOptions options,
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

    private static void CreateDefaultMappings(ModelBuilder modelBuilder)
    {
        var schema = "core";
        modelBuilder.Entity<KkbAuthorizationToken>().ToTable("kkb_authorization_token", schema);
        modelBuilder.Entity<AccountIban>().ToTable("account_iban", schema);
    }

    private static void CreateMsSqlMappings(ModelBuilder modelBuilder)
    {
        var schema = "Core";
        modelBuilder.Entity<KkbAuthorizationToken>().ToTable("KkbAuthorizationToken", schema);
        modelBuilder.Entity<AccountIban>().ToTable("AccountIban", schema);
    }
}