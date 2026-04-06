using System.Reflection;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Accounting.Infrastructure.Persistence;

public class AccountingDbContext : BaseDbContext
{

    public DbSet<ExternalCurrency> ExternalCurrency { get; set; }
    public DbSet<Payment> Payment { get; set; }
    public DbSet<Template> Template { get; set; }
    public DbSet<Invoice> RegisterInvoice { get; set; }
    private readonly IVaultClient _vaultClient;

    public AccountingDbContext(DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService,
        IBus bus,
        IVaultClient vaultClient
    ) : base(options,contextProvider,domainEventService,bus)
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

    private static void CreateDefaultMappings(ModelBuilder modelBuilder)
    {
        string schema = "core";
        modelBuilder.Entity<BankAccount>().ToTable("bank_account", schema);
        modelBuilder.Entity<Customer>().ToTable("customer", schema);
        modelBuilder.Entity<ExternalCurrency>().ToTable("external_currency", schema);
        modelBuilder.Entity<Payment>().ToTable("payment", schema);
        modelBuilder.Entity<Template>().ToTable("template", schema);
        modelBuilder.Entity<Invoice>().ToTable("invoice", schema);
    }
    private static void CreateMsSqlMappings(ModelBuilder modelBuilder)
    {
        string schema = "Core";
        modelBuilder.Entity<BankAccount>().ToTable("BankAccount", schema);
        modelBuilder.Entity<Customer>().ToTable("Customer", schema);
        modelBuilder.Entity<ExternalCurrency>().ToTable("ExternalCurrency", schema);
        modelBuilder.Entity<Payment>().ToTable("Payment", schema);
        modelBuilder.Entity<Template>().ToTable("Template", schema);
        modelBuilder.Entity<Invoice>().ToTable("Invoice", schema);
    }
}