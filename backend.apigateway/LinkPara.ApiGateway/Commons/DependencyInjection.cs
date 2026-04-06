using LinkPara.ApiGateway.Commons.EventBusConfiguration;
using LinkPara.ApiGateway.Services.Billing.HttpClients;
using LinkPara.ApiGateway.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;
using LinkPara.ApiGateway.Services.Cashback.HttpClients;
using LinkPara.ApiGateway.Services.CustomerManagement.HttpClients;
using LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Epin.HttpClients;
using LinkPara.ApiGateway.Services.Fraud.HttpClients;
using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.KKB.HttpClients;
using LinkPara.ApiGateway.Services.KPS.HttpClients;
using LinkPara.ApiGateway.Services.Location.HttpClients;
using LinkPara.ApiGateway.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.Services.MultiFactor;
using LinkPara.ApiGateway.Services.Notification.HttpClients;
using LinkPara.Cache;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.UrlModel;
using MassTransit;

namespace LinkPara.ApiGateway.Commons;

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
        ConfigureServices(services, vaultClient);
        return services;
    }

    private static IServiceCollection ConfigureServices(IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");

        services.AddHttpClient<ICustomerService, CustomerService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CustomerManagement);
        });

        services.AddHttpClient<IUserService, UserService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });
        services.AddHttpClient<IKPSHttpClient, KPSHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.KPS);
        });

        return services;
    }

    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services,
        IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");

        services.AddHttpClient<IUserHttpClient, UserHttpClient>(client =>
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
        services.AddHttpClient<IOtpHttpClient, OtpHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<IWalletHttpClient, WalletHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ISavedAccountHttpClient, SavedAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ICurrencyHttpClient, CurrencyHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITransactionHttpClient, TransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IBankHttpClient, BankHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IBankLogoHttpClient, BankLogoHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IConsentOperationHttpClient, ConsentOperationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IOpenBankingOperationHttpClient, OpenBankingOperationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ILimitHttpClient, LimitHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITierLevelHttpClient, TierLevelHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ISystemBankAccountHttpClient, SystemBankAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IAgreementDocumentHttpClient, AgreementDocumentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<ICountryHttpClient, CountryHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Location);
        });

        services.AddHttpClient<IUserAddressHttpClient, UserAddressHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<IUserPictureHttpClient, UserPictureHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<IQuestionHttpClient, QuestionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<ITransferOrderHttpClient, TransferOrderHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IParameterHttpClient, ParameterHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BusinessParameter);
        });

        services.AddHttpClient<ISectorHttpClient, SectorHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<IInstitutionHttpClient, InstitutionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<IFieldHttpClient, FieldHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<IBillingHttpClient, BillingHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<ICommissionHttpClient, CommissionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<ISavedBillHttpClient, SavedBillHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Billing);
        });

        services.AddHttpClient<IDigitalKycHttpClient, DigitalKycHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.DigitalKyc);
        });

        services.AddHttpClient<IEpinHttpClient, EpinHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Epin);
        });

        services.AddHttpClient<ISourceBankAccountHttpClient, SourceBankAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IEmoneyAccountHttpClient, EmoneyAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IDeviceInfoHttpClient, DeviceInfoHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<ICampaignHttpClient, CampaingHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CampaignManagement);
        });

        services.AddHttpClient<IUserInboxHttpClient, UserInboxHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });
        services.AddHttpClient<ICustomerHttpClient, CustomerHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CustomerManagement);
        });

        services.AddHttpClient<IIWalletAgreementHttpClient, IWalletAgreementHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CampaignManagement);
        });

        services.AddHttpClient<IIWalletLocationtHttpClient, IWalletLocationtHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CampaignManagement);
        });

        services.AddHttpClient<IIWalletCardHttpClient, IWalletCardHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CampaignManagement);
        });

        services.AddHttpClient<IIWalletQrCodeHttpClient, IWalletQrCodeHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CampaignManagement);
        });

        services.AddHttpClient<IMultiFactorTransactionHttpClient, MultiFactorTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.SoftOtp);
        });

        services.AddHttpClient<ITopupHttpClient, TopupHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IMasterpassHttpClient, MasterpassHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ISearchClient, SearchClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Fraud);
        });

        services.AddHttpClient<IOnUsPaymentHttpClient, OnUsPaymentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ISodecHttpClient, SodecHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.DigitalKyc);
        });

        services.AddHttpClient<IScSoftHttpClient, ScSoftHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.DigitalKyc);
        });

        services.AddHttpClient<ICallCenterCustomerAccountHttpClient, CallCenterCustomerAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IPricingProfileHttpClient, PricingProfileHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IKKBHttpClient, KKBHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.KKB);
        });

        services.AddHttpClient<IArksignerHttpClient, ArksignerHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.DigitalKyc);
        });

        services.AddHttpClient<ICashbackHttpClient, CashbackHttpClient>(client =>
       {
           client.BaseAddress = new Uri(serviceUrls.Cashback);
       });

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
}