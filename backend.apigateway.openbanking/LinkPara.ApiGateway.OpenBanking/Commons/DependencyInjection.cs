using LinkPara.ApiGateway.OpenBanking.Commons.EventBusConfiguration;
using LinkPara.ApiGateway.OpenBanking.Services.Emoney.HttpClients;
using LinkPara.Cache;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.UrlModel;
using MassTransit;

namespace LinkPara.ApiGateway.OpenBanking.Commons;

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
        services.AddHttpClient<IAccountServiceProviderHttpClient, AccountServiceProviderHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        return services;
    }
}