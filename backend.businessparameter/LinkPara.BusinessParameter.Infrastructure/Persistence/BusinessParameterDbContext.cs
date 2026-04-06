using System.Reflection;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.BusinessParameter.Infrastructure.Persistence;

public class BusinessParameterDbContext : BaseDbContext
{
    public DbSet<Parameter> Parameter { get; set; }
    public DbSet<ParameterGroup> ParameterGroup { get; set; }
    public DbSet<ParameterTemplate> ParameterTemplate { get; set; }
    public DbSet<ParameterTemplateValue> ParameterTemplateValue { get; set; }

    private readonly IVaultClient _vaultClient;
    public BusinessParameterDbContext(DbContextOptions options,
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
        builder.Entity<Parameter>().ToTable("parameter", schema);
        builder.Entity<ParameterGroup>().ToTable("parameter_group", schema);
        builder.Entity<ParameterTemplate>().ToTable("parameter_template", schema);
        builder.Entity<ParameterTemplateValue>().ToTable("parameter_template_value", schema);
    }

    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        var schema = "Core";
        builder.Entity<Parameter>().ToTable("Parameter", schema);
        builder.Entity<ParameterGroup>().ToTable("ParameterGroup", schema);
        builder.Entity<ParameterTemplate>().ToTable("ParameterTemplate", schema);
        builder.Entity<ParameterTemplateValue>().ToTable("ParameterTemplateValue", schema);
    }
}