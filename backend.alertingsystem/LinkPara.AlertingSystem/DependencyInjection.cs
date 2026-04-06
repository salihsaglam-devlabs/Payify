using LinkPara.AlertingSystem.Commons;
using LinkPara.AlertingSystem.Commons.ErrorQueueMonitor;
using LinkPara.AlertingSystem.Jobs.ErrorQueueMonitor;
using LinkPara.AlertingSystem.Services;
using LinkPara.Cache;
using LinkPara.HttpProviders.Vault;
using MassTransit;

namespace LinkPara.AlertingSystem;

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
        ConfigureServices(services);
        ConfigureMassTransit(services, configuration, vaultClient);

        return services;
    }
    private static void ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<IQueueMonitoring, QueueMonitoring>();
        services.AddSingleton<IEmailService, EmailService>();
    }

     private static IServiceCollection ConfigureMassTransit(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var eventBusSettings = new EventBusSettings();
        if (isLocalConfigurationEnabled)
        {
            configuration.GetSection("LocalConfiguration:EventBusSettings").Bind(eventBusSettings);
        }
        else
        {
            eventBusSettings = vaultClient.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings", null);
        }

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