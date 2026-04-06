using LinkPara.Content.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Content.Infrastructure.Persistence
{
    public class ContentDbContext : BaseDbContext
    {
        public DbSet<DataContainer> DataContainers { get; set; }
        private readonly IVaultClient _vaultClient;


        public ContentDbContext(DbContextOptions options,
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
            builder.Entity<DataContainer>().ToTable("data_containers", schema);
        }

        private static void CreateMsSqlMappings(ModelBuilder builder)
        {
            var schema = "Core";
            builder.Entity<DataContainer>().ToTable("DataContainers", schema);
        }

    }
}