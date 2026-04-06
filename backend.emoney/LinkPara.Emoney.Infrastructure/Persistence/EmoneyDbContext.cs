using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Emoney.Infrastructure.Persistence;

public class EmoneyDbContext : BaseDbContext
{
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;

    public DbSet<Account> Account { get; set; }
    public DbSet<AccountCurrentLevel> AccountCurrentLevel { get; set; }
    public DbSet<AccountCustomTier> AccountCustomTier { get; set; }
    public DbSet<AccountUser> AccountUser { get; set; }
    public DbSet<ApiKey> ApiKey { get; set; }
    public DbSet<Bank> Bank { get; set; }
    public DbSet<BankLogo> BankLogo { get; set; }
    public DbSet<Currency> Currency { get; set; }
    public DbSet<Partner> Partner { get; set; }
    public DbSet<PartnerCounter> PartnerCounter { get; set; }
    public DbSet<PricingProfile> PricingProfile { get; set; }
    public DbSet<PricingProfileItem> PricingProfileItem { get; set; }
    public DbSet<Provision> Provision { get; set; }
    public DbSet<ReturnTransactionRequest> ReturnTransactionRequest { get; set; }
    public DbSet<SavedBankAccount> SavedBankAccount { get; set; }
    public DbSet<SavedWalletAccount> SavedWalletAccount { get; set; }
    public DbSet<TierLevel> TierLevel { get; set; }
    public DbSet<TierLevelUpgradePath> TierLevelUpgradePath { get; set; }
    public DbSet<TierPermission> TierPermission { get; set; }
    public DbSet<Transaction> Transaction { get; set; }
    public DbSet<CallCenterNotificationLog> CallCenterNotificationLog { get; set; }
    public DbSet<TransferOrder> TransferOrder { get; set; }
    public DbSet<Wallet> Wallet { get; set; }
    public DbSet<WithdrawRequest> WithdrawRequest { get; set; }
    public DbSet<AccountActivity> AccountActivity { get; set; }
    public DbSet<PricingCommercial> PricingCommercial { get; set; }
    public DbSet<AccountIban> AccountIban { get; set; }
    public DbSet<AccountKycChange> AccountKycChange { get; set; }
    public DbSet<VirtualIban> VirtualIban { get; set; }
    public DbSet<AccountFinancialInformation> AccountFinancialInformation { get; set; }
    public DbSet<CardTopupRequest> CardTopupRequest { get; set; }
    public DbSet<BulkTransfer> BulkTransfer { get; set; }
    public DbSet<BulkTransferDetail> BulkTransferDetail { get; set; }
    public DbSet<OnUsPaymentRequest> OnUsPaymentRequest { get; set; }
    public DbSet<Chargeback> Chargeback { get; set; }
    public DbSet<ChargebackDocument> ChargebackDocument { get; set; }
    public DbSet<PaymentOrder> PaymentOrder { get; set; }
    public DbSet<ChangedBalanceLog> ChangedBalanceLog { get; set; }
    public DbSet<ManualTransferReference> ManualTransferReference { get; set; }
    public DbSet<WalletBalanceDaily> WalletBalanceDaily { get; set; }
    public DbSet<WalletPaymentRequest> WalletPaymentRequest { get; set; }
    public DbSet<CashbackPaymentRequest> CashbackPaymentRequest { get; set; }
    public EmoneyDbContext(DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService,
        IBus bus, IConfiguration configuration, IVaultClient vaultClient)
        : base(options, contextProvider, domainEventService, bus)
    {
        _configuration = configuration;
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
        const string coreSchema = "core";
        const string limitSchema = "limit";
        const string partnerSchema = "partner";
        builder.Entity<Account>().ToTable("account", coreSchema);
        builder.Entity<AccountCurrentLevel>().ToTable("account_current_level", limitSchema);
        builder.Entity<AccountCustomTier>().ToTable("account_custom_tier", limitSchema);
        builder.Entity<AccountUser>().ToTable("account_user", coreSchema);
        builder.Entity<ApiKey>().ToTable("api_key", partnerSchema);
        builder.Entity<Bank>().ToTable("bank", coreSchema);
        builder.Entity<BankLogo>().ToTable("bank_logo", coreSchema);
        builder.Entity<Currency>().ToTable("currency", coreSchema);
        builder.Entity<Partner>().ToTable("partner", partnerSchema);
        builder.Entity<PartnerCounter>().ToTable("partner_counter", partnerSchema);
        builder.Entity<PricingProfile>().ToTable("pricing_profile", coreSchema);
        builder.Entity<PricingProfileItem>().ToTable("pricing_profile_item", coreSchema);
        builder.Entity<Provision>().ToTable("provision", coreSchema);
        builder.Entity<ReturnTransactionRequest>().ToTable("return_transaction_request", coreSchema);
        builder.Entity<SavedAccount>().ToTable("saved_account", coreSchema);
        builder.Entity<SavedBankAccount>().ToTable("saved_account", coreSchema);
        builder.Entity<SavedWalletAccount>().ToTable("saved_account", coreSchema);
        builder.Entity<TierLevel>().ToTable("tier_level", limitSchema);
        builder.Entity<TierLevelUpgradePath>().ToTable("tier_level_upgrade_path", limitSchema);
        builder.Entity<TierPermission>().ToTable("tier_permission", limitSchema);
        builder.Entity<Transaction>().ToTable("transaction", coreSchema);
        builder.Entity<CallCenterNotificationLog>().ToTable("call_center_notification_log", coreSchema);
        builder.Entity<TransferOrder>().ToTable("transfer_order", coreSchema);
        builder.Entity<Wallet>().ToTable("wallet", coreSchema);
        builder.Entity<WithdrawRequest>().ToTable("withdraw_request", coreSchema);
        builder.Entity<AccountActivity>().ToTable("account_activity", coreSchema);
        builder.Entity<PricingCommercial>().ToTable("pricing_commercial", coreSchema);
        builder.Entity<AccountIban>().ToTable("account_iban", coreSchema);
        builder.Entity<AccountKycChange>().ToTable("account_kyc_change", coreSchema);
        builder.Entity<VirtualIban>().ToTable("virtual_iban", coreSchema);
        builder.Entity<AccountFinancialInformation>().ToTable("account_financial_information", coreSchema);
        builder.Entity<CompanyPool>().ToTable("company_pool", coreSchema);
        builder.Entity<CardTopupRequest>().ToTable("card_topup_request", coreSchema);
        builder.Entity<BulkTransfer>().ToTable("bulk_transfer", coreSchema);
        builder.Entity<BulkTransferDetail>().ToTable("bulk_transfer_detail", coreSchema);
        builder.Entity<OnUsPaymentRequest>().ToTable("onus_payment_request", coreSchema);
        builder.Entity<Chargeback>().ToTable("chargeback", coreSchema);
        builder.Entity<ChargebackDocument>().ToTable("chargeback_document", coreSchema);
        builder.Entity<PaymentOrder>().ToTable("payment_order", coreSchema);
        builder.Entity<ChangedBalanceLog>().ToTable("changed_balance_log", coreSchema);
        builder.Entity<ManualTransferReference>().ToTable("manual_transfer_reference", coreSchema);
        builder.Entity<WalletPaymentRequest>().ToTable("wallet_payment_request", coreSchema);
        builder.Entity<WalletBalanceDaily>().ToTable("wallet_balance_daily", coreSchema);
        builder.Entity<CashbackPaymentRequest>().ToTable("cashback_payment_request", coreSchema);
        builder.Entity<WalletBlockage>().ToTable("wallet_blockage", coreSchema);
        builder.Entity<WalletBlockageDocument>().ToTable("wallet_blockage_document", coreSchema);
    }

    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        const string coreSchema = "Core";
        const string limitSchema = "Limit";
        const string partnerSchema = "Partner";
        builder.Entity<Account>().ToTable("Account", coreSchema);
        builder.Entity<AccountCurrentLevel>().ToTable("AccountCurrentLevel", limitSchema);
        builder.Entity<AccountCustomTier>().ToTable("AccountCustomTier", limitSchema);
        builder.Entity<AccountUser>().ToTable("AccountUser", coreSchema);
        builder.Entity<ApiKey>().ToTable("ApiKey", partnerSchema);
        builder.Entity<Bank>().ToTable("Bank", coreSchema);
        builder.Entity<BankLogo>().ToTable("BankLogo", coreSchema);
        builder.Entity<Currency>().ToTable("Currency", coreSchema);
        builder.Entity<Partner>().ToTable("Partner", partnerSchema);
        builder.Entity<PartnerCounter>().ToTable("PartnerCounter", partnerSchema);
        builder.Entity<PricingProfile>().ToTable("PricingProfile", coreSchema);
        builder.Entity<PricingProfileItem>().ToTable("PricingProfileItem", coreSchema);
        builder.Entity<Provision>().ToTable("Provision", coreSchema);
        builder.Entity<ReturnTransactionRequest>().ToTable("ReturnTransactionRequest", coreSchema);
        builder.Entity<SavedAccount>().ToTable("SavedAccount", coreSchema);
        builder.Entity<SavedBankAccount>().ToTable("SavedAccount", coreSchema);
        builder.Entity<SavedWalletAccount>().ToTable("SavedAccount", coreSchema);
        builder.Entity<TierLevel>().ToTable("TierLevel", limitSchema);
        builder.Entity<TierLevelUpgradePath>().ToTable("TierLevelUpgradePath", limitSchema);
        builder.Entity<TierPermission>().ToTable("TierPermission", limitSchema);
        builder.Entity<Transaction>().ToTable("Transaction", coreSchema);
        builder.Entity<CallCenterNotificationLog>().ToTable("CallCenterNotificationLog", coreSchema);
        builder.Entity<TransferOrder>().ToTable("TransferOrder", coreSchema);
        builder.Entity<Wallet>().ToTable("Wallet", coreSchema);
        builder.Entity<WithdrawRequest>().ToTable("WithdrawRequest", coreSchema);
        builder.Entity<AccountActivity>().ToTable("AccountActivity", coreSchema);
        builder.Entity<PricingCommercial>().ToTable("PricingCommercial", coreSchema);
        builder.Entity<AccountIban>().ToTable("AccountIban", coreSchema);
        builder.Entity<AccountKycChange>().ToTable("AccountKycChange", coreSchema);
        builder.Entity<VirtualIban>().ToTable("VirtualIban", coreSchema);
        builder.Entity<AccountFinancialInformation>().ToTable("AccountFinancialInformation", coreSchema);
        builder.Entity<CompanyPool>().ToTable("CompanyPool", coreSchema);
        builder.Entity<CardTopupRequest>().ToTable("CardTopupRequest", coreSchema);
        builder.Entity<BulkTransfer>().ToTable("BulkTransfer", coreSchema);
        builder.Entity<BulkTransferDetail>().ToTable("BulkTransferDetail", coreSchema);
        builder.Entity<OnUsPaymentRequest>().ToTable("OnusPaymentRequest", coreSchema);
        builder.Entity<Chargeback>().ToTable("Chargeback", coreSchema);
        builder.Entity<ChargebackDocument>().ToTable("ChargebackDocument", coreSchema);
        builder.Entity<PaymentOrder>().ToTable("PaymentOrder", coreSchema);
        builder.Entity<ChangedBalanceLog>().ToTable("ChangedBalanceLog", coreSchema);
        builder.Entity<ManualTransferReference>().ToTable("ManualTransferReference", coreSchema);
        builder.Entity<WalletPaymentRequest>().ToTable("WalletPaymentRequest", coreSchema);
        builder.Entity<WalletBalanceDaily>().ToTable("WalletBalanceDaily", coreSchema);
        builder.Entity<CashbackPaymentRequest>().ToTable("CashbackPaymentRequest", coreSchema);
        builder.Entity<WalletBlockage>().ToTable("WalletBlockage", coreSchema);
        builder.Entity<WalletBlockageDocument>().ToTable("WalletBlockageDocument", coreSchema);
    }

}