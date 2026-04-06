using System.Reflection;
using LinkPara.ContextProvider;
using LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Epin.Infrastructure.Persistence;

public class EpinDbContext : BaseDbContext
{
    private readonly IVaultClient _vaultClient;

    public DbSet<Brand> Brand { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<OrderHistory> OrderHistory { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<Publisher> Publisher { get; set; }
    public DbSet<ReconciliationDetail> ReconciliationDetail { get; set; }
    public DbSet<ReconciliationSummary> ReconciliationSummary { get; set; }

    public EpinDbContext(DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService, IBus bus, IVaultClient vaultClient)
        : base(options, contextProvider, domainEventService, bus)
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
        builder.Entity<Brand>().ToTable("brand", schema);
        builder.Entity<Order>().ToTable("order", schema);
        builder.Entity<OrderHistory>().ToTable("order_history", schema);
        builder.Entity<Product>().ToTable("product", schema);
        builder.Entity<Publisher>().ToTable("publisher", schema);
        builder.Entity<ReconciliationDetail>().ToTable("reconciliation_detail", schema);
        builder.Entity<ReconciliationSummary>().ToTable("reconciliation_summary", schema);
    }
    
    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        var schema = "Core";
        builder.Entity<Brand>().ToTable("Brand", schema);
        builder.Entity<Order>().ToTable("Order", schema);
        builder.Entity<OrderHistory>().ToTable("OrderHistory", schema);
        builder.Entity<Product>().ToTable("Product", schema);
        builder.Entity<Publisher>().ToTable("Publisher", schema);
        builder.Entity<ReconciliationDetail>().ToTable("ReconciliationDetail", schema);
        builder.Entity<ReconciliationSummary>().ToTable("ReconciliationSummary", schema);
    }
}