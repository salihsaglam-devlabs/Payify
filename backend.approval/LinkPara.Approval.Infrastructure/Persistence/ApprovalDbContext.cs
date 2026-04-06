using LinkPara.Approval.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Approval.Infrastructure.Persistence
{
    public class ApprovalDbContext : BaseDbContext
    {

        public DbSet<Case> Case { get; set; }
        public DbSet<MakerChecker> MakerChecker { get; set; }
        public DbSet<Request> Request { get; set; }
        
        private readonly IVaultClient _vaultClient;

        public ApprovalDbContext(DbContextOptions options,
            IContextProvider contextProvider,
            IDomainEventService domainEventService,
            IBus bus,
            IVaultClient vaultClient
        ) : base(options, contextProvider, domainEventService, bus)
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
            string schema;
            var databaseProvider = _vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");
            switch (databaseProvider)
            {
                case "MsSql":
                    #region MsSqlSchema
                    schema = "Core";
                    modelBuilder.Entity<Case>().ToTable("Case", schema);
                    modelBuilder.Entity<MakerChecker>().ToTable("MakerChecker", schema);
                    modelBuilder.Entity<Request>().ToTable("Request", schema);
                    #endregion
                    break;
                default:
                    #region PostgreSchema
                    schema = "core";
                    modelBuilder.Entity<Case>().ToTable("case", schema);
                    modelBuilder.Entity<MakerChecker>().ToTable("maker_checker", schema);
                    modelBuilder.Entity<Request>().ToTable("request", schema);
                    #endregion
                    break;
            }
            base.OnModelCreating(modelBuilder);
        }
    }
}