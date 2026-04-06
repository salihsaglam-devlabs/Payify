using LinkPara.ApiGateway.BackOffice.Commons.EventBusConfiguration;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Approval.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.BackOffice.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.BTrans.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Document.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.IKS.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.KKB.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.KPS.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Location.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.LogConsumers;
using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;
using LinkPara.Cache;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.UrlModel;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.ApiGateway.BackOffice.Commons;

public static class DependencyInjection
{
    public static IVaultClient AddVault(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        var vaultClient = services.ConfigureVaultHttpClient(configuration);
        services.AddHttpClient<IVaultClient, VaultClient>(_ => (VaultClient)vaultClient);

        return vaultClient;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureHttpClients(services, vaultClient);
        ConfigureMasstransit(services, configuration, vaultClient);

        return services;
    }

    public static IServiceCollection ConfigureMasstransit(this IServiceCollection services,
        IConfiguration configuration,
         IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");

        var eventBusSettings = new EventBusSettings();
        if (isLocalConfigurationEnabled) configuration.GetSection("LocalConfiguration:EventBusSettings").Bind(eventBusSettings);
        else eventBusSettings = vaultClient.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings", null);

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri($"rabbitmq://{eventBusSettings.Host}/"),
                     h =>
                     {
                         h.Username(eventBusSettings.Username);
                         h.Password(eventBusSettings.Password);
                     });
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");
        services.AddHttpClient<IUserHttpClient, UserHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<IOtpHttpClient, OtpHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<IAgreementDocumentHttpClient, AgreementDocumentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<IAccountHttpClient, AccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });
        services.AddHttpClient<IAuthHttpClient, AuthHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<ICountryHttpClient, CountryHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Location);
        });

        services.AddHttpClient<IRoleHttpClient, RoleHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });
        services.AddHttpClient<IScreenHttpClient, ScreenHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });
        services.AddHttpClient<IDocumentHttpClient, DocumentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Document);
        });

        services.AddHttpClient<IDocumentTypeHttpClient, DocumentTypeHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Document);
        });

        services.AddHttpClient<ITransactionHttpClient, TransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IPricingProfileHttpClient, PricingProfileHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IAcquireBankHttpClient, AcquireBankHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IBankHttpClient, BankHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantPoolHttpClient, MerchantPoolHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantDocumentHttpClient, MerchantDocumentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantStatementHttpClient, MerchantStatementHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IWalletHttpClient, WalletHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IWalletBlockageHttpClient, WalletBlockageHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IVposHttpClient, VposHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<ICardBinHttpClient, CardBinHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<IDataEncryptionKeyHttpClient, DataEncryptionKeyHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<IPermissionHttpClient, PermissionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<IMerchantHttpClient, MerchantHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<IMerchantReturnPoolHttpClient, MerchantReturnPoolHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<IMerchantIntegratorHttpClient, MerchantIntegratorHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMccHttpClient, MccHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<INaceCodesHttpClient, NaceCodesHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });


        services.AddHttpClient<IAnnulmentHttpClient, AnnulmentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.IKS);
        });

        services.AddHttpClient<IIncomingTransactionHttpClient, IncomingTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IWithdrawRequestHttpClient, WithdrawRequestHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IParameterHttpClient, ParameterHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BusinessParameter);
        });

        services.AddHttpClient<IApprovalHttpClient, ApprovalHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Approval);
        });

        services.AddHttpClient<IParameterGroupHttpClient, ParameterGroupHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BusinessParameter);
        });

        services.AddHttpClient<IParameterTemplateHttpClient, ParameterTemplateHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BusinessParameter);
        });

        services.AddHttpClient<IParameterTemplateValueHttpClient, ParameterTemplateValueHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BusinessParameter);
        });

        services.AddHttpClient<IMerchantHistoryHttpClient, MerchantHistoryHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IBackOfficeHttpClient, BackOfficeHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BackOfficeGateway);
        });

        services.AddHttpClient<ILimitHttpClient, LimitHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITierLevelHttpClient, TierLevelHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITierPermissionHttpClient, TierPermissionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IQuestionHttpClient, QuestionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<ICurrencyHttpClient, CurrencyHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ICostProfileHttpClient, CostPtofileHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IBankApiHttpClient, BankApiHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<ISourceBankAccountHttpClient, SourceBankAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IMoneyTransferTransactionsHttpClient, MoneyTransferTransactionsHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IBankResponseCodeHttpClient, BankResponseCodeHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<ISectorHttpClient, SectorHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<IInstitutionHttpClient, InstitutionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<IBillingHttpClient, BillingHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<IVendorHttpClient, VendorHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<IReconciliationHttpClient, ReconciliationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<ITransferOrderHttpClient, TransferOrderHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ICustomerHttpClient, CustomerHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Accounting);
        });

        services.AddHttpClient<IPaymentHttpClient, PaymentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Accounting);
        });

        services.AddHttpClient<ITimeoutTransactionHttpClient, TimeoutTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantBlockageHttpClient, MerchantBlockageHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IEmoneyPricingProfileHttpClient, EmoneyPricingProfileHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITopupHttpClient, TopupHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IMasterpassHttpClient, MasterpassHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ICommercialPricingHttpClient, CommercialPricingHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IMerchantTransactionHttpClient, MerchantTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPfTransactionHttpClient, PfTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPostingBalanceHttpClient, PostingBalanceHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantUserService, MerchantUserService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPfApiLogHttpClient, PfApiLogHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMoneyTransferReconciliationHttpClient, MoneyTransferReconciliationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IReturnedTransactionHttpClient, ReturnedTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IOrderHttpClient, OrderHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Epin);
        });

        services.AddHttpClient<IProductHttpClient, ProductHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Epin);
        });

        services.AddHttpClient<IPublisherHttpClient, PublisherHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Epin);
        });

        services.AddHttpClient<IBrandHttpClient, BrandHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Epin);
        });

        services.AddHttpClient<IEmoneyStatisticHttpClient, EmoneyStatisticHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
            client.Timeout = TimeSpan.FromSeconds(100);
        });

        services.AddHttpClient<IMoneyTransferStatisticHttpClient, MoneyTransferStatisticHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
            client.Timeout = TimeSpan.FromSeconds(100);
        });

        services.AddHttpClient<IEpinStatisticHttpClient, EpinStatisticHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Epin);
            client.Timeout = TimeSpan.FromSeconds(100);

        });

        services.AddHttpClient<IPfStatisticHttpClient, PfStatisticHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
            client.Timeout = TimeSpan.FromSeconds(100);
        });

        services.AddHttpClient<IBillingStatisticHttpClient, BillingStatisticHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<IEpinReconciliationHttpClient, EpinReconciliationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Epin);
        });

        services.AddHttpClient<IEmoneyAccountHttpClient, EmoneyAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IPostingHttpClient, PostingHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<ICustomerManagementClient, CustomerManagementClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CustomerManagement);
        });

        services.AddHttpClient<IMerchantBusinessPartnerHttpClient, MerchantBusinessPartnerHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IKPSHttpClient, KPSHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.KPS);
        });

        services.AddHttpClient<IBankAccountBalancesHttpClient, BankAccountBalancesHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IChargeHttpClient, ChargeHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CampaignManagement);
        });
        services.AddHttpClient<ISearchClient, SearchClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Fraud);
        });
        services.AddHttpClient<ITransactionMonitoringsClient, TransactionMonitoringsClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Fraud);
        });
        services.AddHttpClient<IBTransDocumentClient, BTransDocumentClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BTrans);
        });

        services.AddHttpClient<IKKBHttpClient, KKBHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.KKB);
        });

        services.AddHttpClient<IMerchantLimitHttpClient, MerchantLimitHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<ICompanyIbanHttpClient, CompanyIbanHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IRepresentativeHttpClient, RepresentativeHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IBranchHttpClient, BranchHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IRepresentativeTransactionHttpClient, RepresentativeTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IBranchTransactionHttpClient, BranchTransactionHttpClient>(client =>
      {
          client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
      });

        services.AddHttpClient<IOperationalTransferBalanceHttpClient, OperationalTransferBalanceHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IOperationalTransferHttpClient, OperationalTransferHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IDueProfileHttpClient, DueProfileHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantDueHttpClient, MerchantDueHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantDeductionHttpClient, MerchantDeductionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IAccountFinancialInfoHttpClient, AccountFinancialInfoHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IUserSimBlockageHttpClient, UserSimBlockageHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<ICompanyPoolHttpClient, CompanyPoolHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IBankLimitHttpClient, BankLimitHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IBankHealthCheckHttpClient, BankHealthCheckHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<ICorporateWalletHttpClient, CorporateWalletHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ILogReportsHtppClient, LogReportsHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.LogConsumers);
        });

        services.AddHttpClient<INotificationTemplatesHttpClient, NotificationTemplatesHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<IAdvancedTemplatesHttpClient, AdvancedNotificationTemplatesHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<INotificationItemsHttpClient, NotificationItemsHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<INotificationEventsHttpClient, NotificationEventsHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<INotificationTemplateParametersHttpClient, NotificationTemplatesParametersHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<INotificationOrdersHttpClient, NotificationOrdersHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<INotificationEventOrdersHttpClient, NotificationEventOrdersHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<INotificationEventItemsHttpClient, NotificationEventItemsHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<IChargebackHttpClient, ChargebackHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IScSoftHttpClient, ScSoftHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.DigitalKyc);
        });

        services.AddHttpClient<IBillingCommissionHttpClient, BillingCommissionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<ISubMerchantUserHttpClient, SubMerchantUserHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<ISubMerchantHttpClient, SubMerchantHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });


        services.AddHttpClient<ICallCenterCustomerAccountHttpClient, CallCenterCustomerAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IMerchantPreApplicationHttpClient, MerchantPreApplicationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<ILinkHttpClient, LinkHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IManualTransfersHttpClient, ManualTransfersHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IParentMerchantHttpClient, ParentMerchantHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IOperationalTransferReportUserHttpClient, OperationalTransferReportUserHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<ICashbackHttpClient, CashbackHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Cashback);
        });

        services.AddHttpClient<ITerminalHistoryHttpClient, TerminalHistoryHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.IKS);
        });

        services.AddHttpClient<IBulkOperationsHttpClient, BulkOperationsHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IDeviceInventoryHttpClient, DeviceInventoryHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPhysicalPosHttpClient, PhysicalPosHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantPhysicalDeviceClient, MerchantPhysicalDeviceClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IUnacceptableTransactionHttpClient, UnacceptableTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPhysicalPosReconciliationHttpClient, PhysicalPosReconciliationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPfReturnTransactionsHttpClient, PfReturnTransactionsHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        return services;
    }
}