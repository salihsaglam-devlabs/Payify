using System.Reflection;
using LinkPara.CampaignManagement.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.CampaignManagement.Infrastructure.Persistence;

public class CampaignManagementDbContext : BaseDbContext
{

    private readonly IVaultClient _vaultClient;

    public CampaignManagementDbContext(DbContextOptions options,
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
        modelBuilder.Entity<AuthorizationToken>().ToTable("authorization_token", schema);
        modelBuilder.Entity<IWalletCard>().ToTable("i_wallet_card", schema);
        modelBuilder.Entity<IWalletQrCode>().ToTable("i_wallet_qr_code", schema);
        modelBuilder.Entity<IWalletCharge>().ToTable("i_wallet_charge", schema);
        modelBuilder.Entity<IWalletCashbackTransaction>().ToTable("i_wallet_cash_back_transaction", schema);
        modelBuilder.Entity<IWalletChargeTransaction>().ToTable("i_wallet_charge_transaction", schema);
    }
    private static void CreateMsSqlMappings(ModelBuilder modelBuilder)
    {
        string schema = "Core";
        modelBuilder.Entity<AuthorizationToken>().ToTable("AuthorizationToken", schema);
        modelBuilder.Entity<IWalletCard>().ToTable("IWalletCard", schema);
        modelBuilder.Entity<IWalletQrCode>().ToTable("IWalletQrCode", schema);
        modelBuilder.Entity<IWalletCharge>().ToTable("IWalletCharge", schema);
        modelBuilder.Entity<IWalletCashbackTransaction>().ToTable("IWalletCashBackTransaction", schema);
        modelBuilder.Entity<IWalletChargeTransaction>().ToTable("IWalletChargeTransaction", schema);
    }
}