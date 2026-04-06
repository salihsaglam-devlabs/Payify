using LinkPara.ApiGateway.Boa.Commons.EventBusConfiguration;
using LinkPara.ApiGateway.Boa.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.Boa.Services.DigitalKyc.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Location.HttpClients;
using LinkPara.ApiGateway.Boa.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;
using LinkPara.Cache;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.UrlModel;
using LinkPara.SystemUser;
using MassTransit;

namespace LinkPara.ApiGateway.Boa.Commons;

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
        services.ConfigureHttpClients(vaultClient);
        services.ConfigureMasstransit(configuration, vaultClient);
        ConfigureServices(services, vaultClient);
        return services;
    }

    private static IServiceCollection ConfigureServices(IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");

        services.AddHttpClient<IUserService, UserService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        return services;
    }

    public static async Task AddApplicationUser(this IServiceCollection services, IVaultClient vaultClient = null)
    {
        services.AddHttpContextAccessor();
        services.AddHttpClient<IUserService, UserService>(client =>
        {
            client.BaseAddress = new Uri(vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Identity"));
        });
        var applicationUserService = await services.ConfigureApplicationUser("BoaGateway", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
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

        services.AddHttpClient<IWalletHttpClient, WalletHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITransactionHttpClient, TransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ILimitHttpClient, LimitHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IDigitalKycHttpClient, DigitalKycHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.DigitalKyc);
        });

        services.AddHttpClient<IEmoneyAccountHttpClient, EmoneyAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IBankHttpClient, BankHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMccHttpClient, MccHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantIntegratorHttpClient, MerchantIntegratorHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IVposHttpClient, VposHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPricingProfileHttpClient, PricingProfileHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantHttpClient, MerchantHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IParameterHttpClient, ParameterHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BusinessParameter);
        });

        services.AddHttpClient<ICountryHttpClient, CountryHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Location);
        });

        services.AddHttpClient<IRolesHttpClient, RolesHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<ISourceBankAccountHttpClient, SourceBankAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
        });

        services.AddHttpClient<IEmoneyBankHttpClient, EmoneyBankHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IBankLogoHttpClient, BankLogoHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ITopupHttpClient, TopupHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<ISystemBankAccountHttpClient, SystemBankAccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
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