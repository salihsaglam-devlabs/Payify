using LinkPara.ContextProvider;
using LinkPara.Documents.Domain.Entities;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Documents.Infrastructure.Persistence;

public class DocumentDbContext : BaseDbContext
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }

    private readonly IVaultClient _vaultClient;

    public DocumentDbContext(
        DbContextOptions options,
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
        modelBuilder.Entity<Document>().ToTable("document", schema);
        modelBuilder.Entity<DocumentType>().ToTable("document_type", schema);
    }

    private static void CreateMsSqlMappings(ModelBuilder modelBuilder)
    {
        var schema = "Core";
        modelBuilder.Entity<Document>().ToTable("Document", schema);
        modelBuilder.Entity<DocumentType>().ToTable("DocumentType", schema);
    }
}