using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace LinkPara.PF.Infrastructure.Persistence;

public class PfDbContext : BaseDbContext
{
    public DbSet<Merchant> Merchant { get; set; }
    public DbSet<Customer> Customer { get; set; }
    public DbSet<ContactPerson> ContactPerson { get; set; }
    public DbSet<MerchantDocument> MerchantDocument { get; set; }
    public DbSet<MerchantBlockage> MerchantBlockage { get; set; }
    public DbSet<MerchantBlockageDetail> MerchantBlockageDetail { get; set; }
    public DbSet<MerchantApiKey> MerchantApiKey { get; set; }
    public DbSet<MerchantBankAccount> MerchantBankAccount { get; set; }
    public DbSet<MerchantEmail> MerchantEmail { get; set; }
    public DbSet<MerchantScore> MerchantScore { get; set; }
    public DbSet<MerchantPool> MerchantPool { get; set; }
    public DbSet<MerchantUser> MerchantUser { get; set; }
    public DbSet<MerchantVpos> MerchantVpos { get; set; }
    public DbSet<Bank> Bank { get; set; }
    public DbSet<BankApiKey> BankApiKey { get; set; }
    public DbSet<CardToken> CardToken { get; set; }
    public DbSet<AcquireBank> AcquireBank { get; set; }
    public DbSet<CardBin> CardBin { get; set; }
    public DbSet<Currency> Currency { get; set; }
    public DbSet<CostProfile> CostProfile { get; set; }
    public DbSet<CostProfileItem> CostProfileItem { get; set; }
    public DbSet<CostProfileInstallment> CostProfileInstallment { get; set; }
    public DbSet<Vpos> Vpos { get; set; }
    public DbSet<VposBankApiInfo> VposBankApiInfo { get; set; }
    public DbSet<VposCurrency> VposCurrency { get; set; }
    public DbSet<PricingProfile> PricingProfile { get; set; }
    public DbSet<PricingProfileItem> PricingProfileItem { get; set; }
    public DbSet<PricingProfileInstallment> PricingProfileInstallment { get; set; }
    public DbSet<Mcc> Mcc { get; set; }
    public DbSet<BankTransaction> BankTransaction { get; set; }
    public DbSet<MerchantTransaction> MerchantTransaction { get; set; }
    public DbSet<MerchantLimit> MerchantLimit { get; set; }
    public DbSet<MerchantHistory> MerchantHistory { get; set; }
    public DbSet<MerchantIntegrator> MerchantIntegrator { get; set; }
    public DbSet<TimeoutTransaction> TimeoutTransaction { get; set; }
    public DbSet<ThreeDVerification> ThreeDVerification { get; set; }
    public DbSet<BankResponseCode> BankResponseCode { get; set; }
    public DbSet<MerchantResponseCode> MerchantResponseCode { get; set; }
    public DbSet<MerchantApiValidationLog> MerchantApiValidationLog { get; set; }
    public DbSet<CardLoyalty> CardLoyalty { get; set; }
    public DbSet<CardLoyaltyException> CardLoyaltyException { get; set; }
    public DbSet<PostingTransaction> PostingTransaction { get; set; }
    public DbSet<PostingTransferError> PostingTransferError { get; set; }
    public DbSet<MerchantCounter> MerchantCounter { get; set; }
    public DbSet<MerchantApiLog> MerchantApiLog { get; set; }
    public DbSet<MerchantDailyUsage> MerchantDailyUsage { get; set; }
    public DbSet<MerchantMonthlyUsage> MerchantMonthlyUsage { get; set; }
    public DbSet<PostingItem> PostingItem { get; set; }
    public DbSet<PostingBalance> PostingBalance { get; set; }
    public DbSet<PostingBankBalance> PostingBankBalance { get; set; }
    public DbSet<PostingBatchStatus> PostingBatchStatus { get; set; }
    public DbSet<PostingBill> PostingBill { get; set; }
    public DbSet<MerchantBusinessPartner> MerchantBusinessPartner { get; set; }
    public DbSet<MerchantReturnPool> MerchantReturnPool { get; set; }
    public DbSet<MerchantDue> MerchantDue { get; set; }
    public DbSet<MerchantDeduction> MerchantDeduction { get; set; }
    public DbSet<MerchantStatement> MerchantStatement { get; set; }
    public DbSet<MerchantContent> MerchantContent { get; set; }
    public DbSet<MerchantContentVersion> MerchantContentVersion { get; set; }
    public DbSet<MerchantLogo> MerchantLogo { get; set; }
    public DbSet<ApiResponseCode> ApiResponseCode { get; set; }
    public DbSet<Link> Link { get; set; }
    public DbSet<LinkInstallment> LinkInstallment { get; set; }
    public DbSet<LinkCustomer> LinkCustomer { get; set; }
    public DbSet<LinkTransaction> LinkTransaction { get; set; }
    public DbSet<HostedPayment> HostedPayment { get; set; }
    public DbSet<HostedPaymentInstallment> HostedPaymentInstallment { get; set; }
    public DbSet<HostedPaymentTransaction> HostedPaymentTransaction { get; set; }
    public DbSet<DeductionTransaction> DeductionTransaction { get; set; }
    public DbSet<DueProfile> DueProfile { get; set; }
    public DbSet<OnUsPayment> OnUsPayment { get; set; }
    public DbSet<BankLimit> BankLimit { get; set; }
    public DbSet<BankHealthCheck> BankHealthCheck { get; set; }
    public DbSet<BankHealthCheckTransaction> BankHealthCheckTransaction { get; set; }
    public DbSet<MerchantWallet> MerchantWallet { get; set; }
    public DbSet<SubMerchant> SubMerchant { get; set; }
    public DbSet<SubMerchantDocument> SubMerchantDocument { get; set; }
    public DbSet<SubMerchantLimit> SubMerchantLimit { get; set; }
    public DbSet<SubMerchantMonthlyUsage> SubMerchantMonthlyUsage { get; set; }
    public DbSet<SubMerchantDailyUsage> SubMerchantDailyUsage { get; set; }
    public DbSet<SubMerchantUser> SubMerchantUser { get; set; }
    public DbSet<MerchantPreApplication> MerchantPreApplication { get; set; }
    public DbSet<MerchantPreApplicationHistory> MerchantPreApplicationHistory { get; set; }
    public DbSet<PostingAdditionalTransaction> PostingAdditionalTransaction { get; set; }
    public DbSet<PostingPfProfit> PostingPfProfit { get; set; }
    public DbSet<PostingPfProfitDetail> PostingPfProfitDetail { get; set; }
    public DbSet<DeviceInventory> DeviceInventory { get; set; }
    public DbSet<MerchantPhysicalDevice> MerchantPhysicalDevice { get; set; }
    public DbSet<MerchantPhysicalPos> MerchantPhysicalPos { get; set; }
    public DbSet<Domain.Entities.PhysicalPos.PhysicalPos> PhysicalPos { get; set; }
    public DbSet<PhysicalPosCurrency> PhysicalPosCurrency { get; set; }
    public DbSet<MerchantDeviceApiKey> MerchantDeviceApiKey { get; set; }
    public DbSet<PhysicalPosEndOfDay> PhysicalPosEndOfDay { get; set; }
    public DbSet<PhysicalPosReconciliationTransaction> PhysicalPosReconciliationTransaction { get; set; }
    public DbSet<PhysicalPosUnacceptableTransaction> PhysicalPosUnacceptableTransaction { get; set; }
    public DbSet<DeviceInventoryHistory> DeviceInventoryHistory { get; set; }
    public DbSet<Nace> Nace { get; set; }
    public DbSet<MerchantInstallmentTransaction> MerchantInstallmentTransaction { get; set; }

    private readonly IVaultClient _vaultClient;
    public PfDbContext(DbContextOptions options,
        IContextProvider contextProvider,
        IDomainEventService domainEventService,
        IBus bus,
        IConfiguration configuration, 
        IVaultClient vaultClient)
        : base(options, contextProvider, domainEventService,bus)
    {
        _vaultClient = vaultClient;

        ChangeTracker.LazyLoadingEnabled = false;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);

        var databaseProvider = _vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        string bankSchema, cardSchema, coreSchema, limitSchema, merchantSchema, subMerchantSchema, postingSchema, vposSchema, apiSchema, linkSchema, hostedPaymentSchema, physicalSchema;

        switch (databaseProvider)
        {
            default:
                #region PostgreSchema
                bankSchema = "bank";
                cardSchema = "card";
                coreSchema = "core";
                limitSchema = "limit";
                merchantSchema = "merchant";
                subMerchantSchema = "submerchant";
                postingSchema = "posting";
                vposSchema = "vpos";
                apiSchema = "api";
                linkSchema = "link";
                hostedPaymentSchema = "hpp";
                physicalSchema = "physical";

                //physical schema
                modelBuilder.Entity<Domain.Entities.PhysicalPos.PhysicalPos>().ToTable("physical_pos", physicalSchema);
                modelBuilder.Entity<DeviceInventory>().ToTable("device_inventory", physicalSchema);                
                modelBuilder.Entity<DeviceInventoryHistory>().ToTable("device_inventory_history", physicalSchema);                
                modelBuilder.Entity<PhysicalPosCurrency>().ToTable("physical_pos_currency", physicalSchema);                
                modelBuilder.Entity<PhysicalPosEndOfDay>().ToTable("end_of_day", physicalSchema);                
                modelBuilder.Entity<PhysicalPosReconciliationTransaction>().ToTable("reconciliation_transaction", physicalSchema);                
                modelBuilder.Entity<PhysicalPosUnacceptableTransaction>().ToTable("unacceptable_transaction", physicalSchema);                

                //bank schema
                modelBuilder.Entity<AcquireBank>().ToTable("acquire_bank", bankSchema);
                modelBuilder.Entity<BankApiKey>().ToTable("api_key", bankSchema);
                modelBuilder.Entity<Bank>().ToTable("bank", bankSchema);
                modelBuilder.Entity<BankResponseCode>().ToTable("response_code", bankSchema);
                modelBuilder.Entity<BankTransaction>().ToTable("transaction", bankSchema);
                modelBuilder.Entity<BankLimit>().ToTable("limit", bankSchema);
                modelBuilder.Entity<BankHealthCheck>().ToTable("health_check", bankSchema);
                modelBuilder.Entity<BankHealthCheckTransaction>().ToTable("health_check_transaction", bankSchema);

                //card schema
                modelBuilder.Entity<CardBin>().ToTable("bin", cardSchema);
                modelBuilder.Entity<CardLoyalty>().ToTable("loyalty", cardSchema);
                modelBuilder.Entity<CardLoyaltyException>().ToTable("loyalty_exception", cardSchema);
                modelBuilder.Entity<CardToken>().ToTable("token", cardSchema);

                //core schema
                modelBuilder.Entity<ContactPerson>().ToTable("contact_person", coreSchema);
                modelBuilder.Entity<CostProfile>().ToTable("cost_profile", coreSchema);
                modelBuilder.Entity<CostProfileItem>().ToTable("cost_profile_item", coreSchema);
                modelBuilder.Entity<CostProfileInstallment>().ToTable("cost_profile_installment", coreSchema);
                modelBuilder.Entity<Currency>().ToTable("currency", coreSchema);
                modelBuilder.Entity<Customer>().ToTable("customer", coreSchema);
                modelBuilder.Entity<PricingProfile>().ToTable("pricing_profile", coreSchema);
                modelBuilder.Entity<PricingProfileItem>().ToTable("pricing_profile_item", coreSchema);
                modelBuilder.Entity<PricingProfileInstallment>().ToTable("pricing_profile_installment", coreSchema);
                modelBuilder.Entity<ThreeDVerification>().ToTable("three_d_verification", coreSchema);
                modelBuilder.Entity<TimeoutTransaction>().ToTable("time_out_transaction", coreSchema);
                modelBuilder.Entity<DueProfile>().ToTable("due_profile", coreSchema);
                modelBuilder.Entity<OnUsPayment>().ToTable("on_us_payment", coreSchema);

                //limit schema
                modelBuilder.Entity<MerchantDailyUsage>().ToTable("merchant_daily_usage", limitSchema);
                modelBuilder.Entity<MerchantLimit>().ToTable("merchant_limit", limitSchema);
                modelBuilder.Entity<MerchantMonthlyUsage>().ToTable("merchant_monthly_usage", limitSchema);

                //api schema
                modelBuilder.Entity<MerchantApiLog>().ToTable("log", apiSchema);
                modelBuilder.Entity<MerchantApiValidationLog>().ToTable("validation_log", apiSchema);
                modelBuilder.Entity<ApiResponseCode>().ToTable("response_code", apiSchema);

                //merchant schema
                modelBuilder.Entity<MerchantApiKey>().ToTable("api_key", merchantSchema);
                modelBuilder.Entity<MerchantBankAccount>().ToTable("bank_account", merchantSchema);
                modelBuilder.Entity<MerchantBlockage>().ToTable("blockage", merchantSchema);
                modelBuilder.Entity<MerchantBlockageDetail>().ToTable("blockage_detail", merchantSchema);
                modelBuilder.Entity<MerchantCounter>().ToTable("counter", merchantSchema);
                modelBuilder.Entity<MerchantDocument>().ToTable("document", merchantSchema);
                modelBuilder.Entity<MerchantEmail>().ToTable("email", merchantSchema);
                modelBuilder.Entity<MerchantHistory>().ToTable("history", merchantSchema);
                modelBuilder.Entity<MerchantIntegrator>().ToTable("integrator", merchantSchema);
                modelBuilder.Entity<Mcc>().ToTable("mcc", merchantSchema);
                modelBuilder.Entity<Merchant>().ToTable("merchant", merchantSchema);
                modelBuilder.Entity<MerchantPool>().ToTable("pool", merchantSchema);
                modelBuilder.Entity<MerchantResponseCode>().ToTable("response_code", merchantSchema);
                modelBuilder.Entity<MerchantScore>().ToTable("score", merchantSchema);
                modelBuilder.Entity<MerchantTransaction>().ToTable("transaction", merchantSchema);
                modelBuilder.Entity<MerchantUser>().ToTable("user", merchantSchema);
                modelBuilder.Entity<MerchantVpos>().ToTable("vpos", merchantSchema);
                modelBuilder.Entity<MerchantBusinessPartner>().ToTable("business_partner", merchantSchema);
                modelBuilder.Entity<MerchantReturnPool>().ToTable("merchant_return_pool", merchantSchema);
                modelBuilder.Entity<MerchantDue>().ToTable("merchant_due", merchantSchema);
                modelBuilder.Entity<MerchantDeduction>().ToTable("merchant_deduction", merchantSchema);
                modelBuilder.Entity<MerchantStatement>().ToTable("merchant_statement", merchantSchema);
                modelBuilder.Entity<MerchantContent>().ToTable("merchant_content", merchantSchema);
                modelBuilder.Entity<MerchantContentVersion>().ToTable("merchant_content_version", merchantSchema);
                modelBuilder.Entity<MerchantLogo>().ToTable("merchant_logo", merchantSchema);
                modelBuilder.Entity<DeductionTransaction>().ToTable("deduction_transaction", merchantSchema);
                modelBuilder.Entity<MerchantWallet>().ToTable("wallet", merchantSchema);
                modelBuilder.Entity<MerchantPhysicalDevice>().ToTable("merchant_physical_device", merchantSchema);
                modelBuilder.Entity<MerchantPhysicalPos>().ToTable("merchant_physical_pos", merchantSchema);
                modelBuilder.Entity<MerchantDeviceApiKey>().ToTable("merchant_device_api_key", merchantSchema);
                modelBuilder.Entity<Nace>().ToTable("nace", merchantSchema);
                modelBuilder.Entity<MerchantInstallmentTransaction>().ToTable("installment_transaction", merchantSchema);

                //submerchant schema
                modelBuilder.Entity<SubMerchant>().ToTable("sub_merchant", subMerchantSchema);
                modelBuilder.Entity<SubMerchantDocument>().ToTable("document", subMerchantSchema);
                modelBuilder.Entity<SubMerchantLimit>().ToTable("limit", subMerchantSchema);
                modelBuilder.Entity<SubMerchantUser>().ToTable("user", subMerchantSchema);
                modelBuilder.Entity<SubMerchantDailyUsage>().ToTable("daily_usage", subMerchantSchema);
                modelBuilder.Entity<SubMerchantMonthlyUsage>().ToTable("monthly_usage", subMerchantSchema);

                //posting schema
                modelBuilder.Entity<PostingBalance>().ToTable("balance", postingSchema);
                modelBuilder.Entity<PostingBankBalance>().ToTable("bank_balance", postingSchema);
                modelBuilder.Entity<PostingBatchStatus>().ToTable("batch_status", postingSchema);
                modelBuilder.Entity<PostingItem>().ToTable("item", postingSchema);
                modelBuilder.Entity<PostingTransaction>().ToTable("transaction", postingSchema);
                modelBuilder.Entity<PostingTransferError>().ToTable("transfer_error", postingSchema);
                modelBuilder.Entity<PostingBill>().ToTable("posting_bill", postingSchema);
                modelBuilder.Entity<PostingAdditionalTransaction>().ToTable("posting_additional_transaction", postingSchema);
                modelBuilder.Entity<PostingPfProfit>().ToTable("pf_profit", postingSchema);
                modelBuilder.Entity<PostingPfProfitDetail>().ToTable("pf_profit_detail", postingSchema);

                //limit schema
                modelBuilder.Entity<VposBankApiInfo>().ToTable("bank_api_info", vposSchema);
                modelBuilder.Entity<VposCurrency>().ToTable("currency", vposSchema);
                modelBuilder.Entity<Vpos>().ToTable("vpos", vposSchema);

                //link schema
                modelBuilder.Entity<Link>().ToTable("link", linkSchema);
                modelBuilder.Entity<LinkInstallment>().ToTable("link_installment", linkSchema);
                modelBuilder.Entity<LinkCustomer>().ToTable("link_customer", linkSchema);
                modelBuilder.Entity<LinkTransaction>().ToTable("link_transaction", linkSchema);
                
                //hosted payment schema
                modelBuilder.Entity<HostedPayment>().ToTable("hosted_payment", hostedPaymentSchema);
                modelBuilder.Entity<HostedPaymentInstallment>().ToTable("hosted_payment_installment", hostedPaymentSchema);
                modelBuilder.Entity<HostedPaymentTransaction>().ToTable("hosted_payment_transaction", hostedPaymentSchema);
                
                //merchant pre application
                modelBuilder.Entity<MerchantPreApplication>().ToTable("merchant_pre_application", merchantSchema);
                modelBuilder.Entity<MerchantPreApplicationHistory>().ToTable("merchant_pre_application_history", merchantSchema);

                #endregion
                break;
            
            case "MsSql":
                #region MsSqlSchema
                bankSchema = "Bank";
                cardSchema = "Card";
                coreSchema = "Core";
                limitSchema = "Limit";
                merchantSchema = "Merchant";
                subMerchantSchema = "Submerchant";
                postingSchema = "Posting";
                vposSchema = "Vpos";
                apiSchema = "Api";
                linkSchema = "Link";
                hostedPaymentSchema = "Hpp";
                physicalSchema = "Physical";

                //physical schema
                modelBuilder.Entity<DeviceInventory>().ToTable("DeviceInventory", physicalSchema);
                modelBuilder.Entity<DeviceInventoryHistory>().ToTable("DeviceInventoryHistory", physicalSchema);
                modelBuilder.Entity<Domain.Entities.PhysicalPos.PhysicalPos>().ToTable("PhysicalPos", physicalSchema);                
                modelBuilder.Entity<PhysicalPosCurrency>().ToTable("PhysicalPosCurrency", physicalSchema);   
                modelBuilder.Entity<PhysicalPosEndOfDay>().ToTable("EndOfDay", physicalSchema);                
                modelBuilder.Entity<PhysicalPosReconciliationTransaction>().ToTable("ReconciliationTransaction", physicalSchema);       
                modelBuilder.Entity<PhysicalPosUnacceptableTransaction>().ToTable("UnacceptableTransaction", physicalSchema);       

                //bank schema
                modelBuilder.Entity<AcquireBank>().ToTable("AcquireBank", bankSchema);
                modelBuilder.Entity<BankApiKey>().ToTable("ApiKey", bankSchema);
                modelBuilder.Entity<Bank>().ToTable("Bank", bankSchema);
                modelBuilder.Entity<BankResponseCode>().ToTable("ResponseCode", bankSchema);
                modelBuilder.Entity<BankTransaction>().ToTable("Transaction", bankSchema);
                modelBuilder.Entity<BankLimit>().ToTable("Limit", bankSchema);
                modelBuilder.Entity<BankHealthCheck>().ToTable("HealthCheck", bankSchema);
                modelBuilder.Entity<BankHealthCheckTransaction>().ToTable("HealthCheckTransaction", bankSchema);

                //card schema
                modelBuilder.Entity<CardBin>().ToTable("Bin", cardSchema);
                modelBuilder.Entity<CardLoyalty>().ToTable("Loyalty", cardSchema);
                modelBuilder.Entity<CardLoyaltyException>().ToTable("LoyaltyException", cardSchema);
                modelBuilder.Entity<CardToken>().ToTable("Token", cardSchema);

                //core schema
                modelBuilder.Entity<ContactPerson>().ToTable("ContactPerson", coreSchema);
                modelBuilder.Entity<CostProfile>().ToTable("CostProfile", coreSchema);
                modelBuilder.Entity<CostProfileItem>().ToTable("CostProfileItem", coreSchema);
                modelBuilder.Entity<CostProfileInstallment>().ToTable("CostProfileInstallment", coreSchema);
                modelBuilder.Entity<Currency>().ToTable("Currency", coreSchema);
                modelBuilder.Entity<Customer>().ToTable("Customer", coreSchema);
                modelBuilder.Entity<PricingProfile>().ToTable("PricingProfile", coreSchema);
                modelBuilder.Entity<PricingProfileItem>().ToTable("PricingProfileItem", coreSchema);
                modelBuilder.Entity<PricingProfileInstallment>().ToTable("PricingProfileInstallment", coreSchema);
                modelBuilder.Entity<ThreeDVerification>().ToTable("ThreeDVerification", coreSchema);
                modelBuilder.Entity<TimeoutTransaction>().ToTable("TimeoutTransaction", coreSchema);
                modelBuilder.Entity<DueProfile>().ToTable("DueProfile", coreSchema);
                modelBuilder.Entity<OnUsPayment>().ToTable("OnUsPayment", coreSchema);

                //limit schema
                modelBuilder.Entity<MerchantDailyUsage>().ToTable("MerchantDailyUsage", limitSchema);
                modelBuilder.Entity<MerchantLimit>().ToTable("MerchantLimit", limitSchema);
                modelBuilder.Entity<MerchantMonthlyUsage>().ToTable("MerchantMonthlyUsage", limitSchema);

                //api schema
                modelBuilder.Entity<MerchantApiLog>().ToTable("Log", apiSchema);
                modelBuilder.Entity<MerchantApiValidationLog>().ToTable("ValidationLog", apiSchema);
                modelBuilder.Entity<ApiResponseCode>().ToTable("ResponseCode", apiSchema);

                //merchant schema
                modelBuilder.Entity<MerchantApiKey>().ToTable("ApiKey", merchantSchema);
                modelBuilder.Entity<MerchantBankAccount>().ToTable("BankAccount", merchantSchema);
                modelBuilder.Entity<MerchantBlockage>().ToTable("Blockage", merchantSchema);
                modelBuilder.Entity<MerchantBlockageDetail>().ToTable("BlockageDetail", merchantSchema);
                modelBuilder.Entity<MerchantCounter>().ToTable("Counter", merchantSchema);
                modelBuilder.Entity<MerchantDocument>().ToTable("Document", merchantSchema);
                modelBuilder.Entity<MerchantEmail>().ToTable("Email", merchantSchema);
                modelBuilder.Entity<MerchantHistory>().ToTable("History", merchantSchema);
                modelBuilder.Entity<MerchantIntegrator>().ToTable("Integrator", merchantSchema);
                modelBuilder.Entity<Mcc>().ToTable("Mcc", merchantSchema);
                modelBuilder.Entity<Merchant>().ToTable("Merchant", merchantSchema);
                modelBuilder.Entity<MerchantPool>().ToTable("Pool", merchantSchema);
                modelBuilder.Entity<MerchantResponseCode>().ToTable("ResponseCode", merchantSchema);
                modelBuilder.Entity<MerchantScore>().ToTable("Score", merchantSchema);
                modelBuilder.Entity<MerchantTransaction>().ToTable("Transaction", merchantSchema);
                modelBuilder.Entity<MerchantUser>().ToTable("User", merchantSchema);
                modelBuilder.Entity<MerchantVpos>().ToTable("Vpos", merchantSchema);
                modelBuilder.Entity<MerchantBusinessPartner>().ToTable("BusinessPartner", merchantSchema);
                modelBuilder.Entity<MerchantDue>().ToTable("MerchantDue", merchantSchema);
                modelBuilder.Entity<MerchantReturnPool>().ToTable("MerchantReturnPool", merchantSchema);
                modelBuilder.Entity<MerchantDeduction>().ToTable("MerchantDeduction", merchantSchema);
                modelBuilder.Entity<MerchantStatement>().ToTable("MerchantStatement", merchantSchema);
                modelBuilder.Entity<MerchantContent>().ToTable("MerchantContent", merchantSchema);
                modelBuilder.Entity<MerchantContentVersion>().ToTable("MerchantContentVersion", merchantSchema);
                modelBuilder.Entity<MerchantLogo>().ToTable("MerchantLogo", merchantSchema);
                modelBuilder.Entity<DeductionTransaction>().ToTable("DeductionTransaction", merchantSchema);
                modelBuilder.Entity<MerchantWallet>().ToTable("Wallet", merchantSchema);
                modelBuilder.Entity<MerchantPhysicalDevice>().ToTable("MerchantPhysicalDevice", merchantSchema);
                modelBuilder.Entity<MerchantPhysicalPos>().ToTable("MerchantPhysicalPos", merchantSchema);
                modelBuilder.Entity<MerchantDeviceApiKey>().ToTable("MerchantDeviceApiKey", merchantSchema);
                modelBuilder.Entity<Nace>().ToTable("Nace", merchantSchema);
                modelBuilder.Entity<MerchantInstallmentTransaction>().ToTable("InstallmentTransaction", merchantSchema);

                //submerchant schema
                modelBuilder.Entity<SubMerchant>().ToTable("SubMerchant", subMerchantSchema);
                modelBuilder.Entity<SubMerchantDocument>().ToTable("Document", subMerchantSchema);
                modelBuilder.Entity<SubMerchantLimit>().ToTable("Limit", subMerchantSchema);
                modelBuilder.Entity<SubMerchantUser>().ToTable("User", subMerchantSchema);
                modelBuilder.Entity<SubMerchantDailyUsage>().ToTable("DailyUsage", subMerchantSchema);
                modelBuilder.Entity<SubMerchantMonthlyUsage>().ToTable("MonthlyUsage", subMerchantSchema);

                //posting schema
                modelBuilder.Entity<PostingBalance>().ToTable("Balance", postingSchema);
                modelBuilder.Entity<PostingBankBalance>().ToTable("BankBalance", postingSchema);
                modelBuilder.Entity<PostingBatchStatus>().ToTable("BatchStatus", postingSchema);
                modelBuilder.Entity<PostingItem>().ToTable("Item", postingSchema);
                modelBuilder.Entity<PostingTransaction>().ToTable("Transaction", postingSchema);
                modelBuilder.Entity<PostingTransferError>().ToTable("TransferError", postingSchema);
                modelBuilder.Entity<PostingBill>().ToTable("PostingBill", postingSchema);
                modelBuilder.Entity<PostingAdditionalTransaction>().ToTable("PostingAdditionalTransaction", postingSchema);
                modelBuilder.Entity<PostingPfProfit>().ToTable("PfProfit", postingSchema);
                modelBuilder.Entity<PostingPfProfitDetail>().ToTable("PfProfitDetail", postingSchema);

                //limit schema
                modelBuilder.Entity<VposBankApiInfo>().ToTable("BankApiInfo", vposSchema);
                modelBuilder.Entity<VposCurrency>().ToTable("Currency", vposSchema);
                modelBuilder.Entity<Vpos>().ToTable("Vpos", vposSchema);

                //link schema
                modelBuilder.Entity<Link>().ToTable("Link", linkSchema);
                modelBuilder.Entity<LinkInstallment>().ToTable("LinkInstallment", linkSchema);
                modelBuilder.Entity<LinkCustomer>().ToTable("LinkCustomer", linkSchema);
                modelBuilder.Entity<LinkTransaction>().ToTable("LinkTransaction", linkSchema);
                
                //hosted payment schema
                modelBuilder.Entity<HostedPayment>().ToTable("HostedPayment", hostedPaymentSchema);
                modelBuilder.Entity<HostedPaymentInstallment>().ToTable("HostedPaymentInstallment", hostedPaymentSchema);
                modelBuilder.Entity<HostedPaymentTransaction>().ToTable("HostedPaymentTransaction", hostedPaymentSchema);
                
                //merchant pre application
                modelBuilder.Entity<MerchantPreApplication>().ToTable("MerchantPreApplication", merchantSchema);
                modelBuilder.Entity<MerchantPreApplicationHistory>().ToTable("MerchantPreApplicationHistory", merchantSchema);
                #endregion
                break;
        }
    }
}