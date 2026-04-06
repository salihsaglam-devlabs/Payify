using Microsoft.EntityFrameworkCore;
using System.Reflection;
using LinkPara.SharedModels.Persistence;
using LinkPara.ContextProvider;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Calendar.Infrastructure.Persistence;

public class CalendarDbContext : BaseDbContext
{
    public DbSet<Holiday> Holiday { get; set; }
    public DbSet<HolidayDetail> HolidayDetail { get; set; }

    private readonly IVaultClient _vaultClient;

    public CalendarDbContext(DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService,
        IBus bus,
        IVaultClient vaultClient)
        : base(options, contextProvider, domainEventService, bus)
    {
       _vaultClient = vaultClient;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);

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

    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        string schema = "Core";
        builder.Entity<Holiday>().ToTable("Holiday", schema);
        builder.Entity<HolidayDetail>().ToTable("HolidayDetail", schema);
    }

    private static void CreateDefaultMappings(ModelBuilder builder)
    {
        string schema = "core";
        builder.Entity<Holiday>().ToTable("holiday", schema);
        builder.Entity<HolidayDetail>().ToTable("holiday_detail", schema);
    }
}