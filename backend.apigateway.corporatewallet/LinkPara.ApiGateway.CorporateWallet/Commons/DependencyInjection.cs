using LinkPara.ApiGateway.CorporateWallet.Commons.EventBusConfiguration;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Document.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Location.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.Cache;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.UrlModel;
using MassTransit;

namespace LinkPara.ApiGateway.CorporateWallet.Commons;

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

    public static IServiceCollection ConfigureMasstransit(IServiceCollection services,
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

        services.AddHttpClient<INotificationHttpClient, NotificationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
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

        services.AddHttpClient<IParameterHttpClient, ParameterHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BusinessParameter);
        });

        services.AddHttpClient<ICurrencyHttpClient, CurrencyHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ICompanyPoolHttpClient, CompanyPoolHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IScreenHttpClient, ScreenHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<IWalletHttpClient, WalletHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
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

        services.AddHttpClient<IEmoneyAccountHttpClient, EmoneyAccountHttpClient>(client =>
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

        services.AddHttpClient<ILimitHttpClient, LimitHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ISavedAccountHttpClient, SavedAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ISystemBankAccountHttpClient, SystemBankAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITierLevelHttpClient, TierLevelHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITransactionHttpClient, TransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITransferOrderHttpClient, TransferOrderHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IWalletHttpClient, WalletHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IAgreementDocumentHttpClient, AgreementDocumentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<ISourceBankAccountHttpClient, SourceBankAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IBulkTransferHttpClient, BulkTransferHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });


        services.AddHttpClient<ICorporateWalletHttpClient, CorporateWalletHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IDocumentHttpClient, DocumentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Document);
        });
        return services;
    }
}