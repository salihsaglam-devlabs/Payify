using LinkPara.SystemUser;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.EventBusConfiguration;
using LinkPara.IKS.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LinkPara.IKS.Infrastructure.Services;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.HttpProviders.Vault;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Identity;
using LinkPara.SharedModels.Migration;
using LinkPara.Cache;
using Microsoft.EntityFrameworkCore;
using LinkPara.Audit;
using LinkPara.SharedModels.Persistence;
using LinkPara.IKS.Infrastructure.Consumers.CronJobs;
using LinkPara.HttpProviders.PF;
using LinkPara.HttpProviders.BusinessParameter;

namespace LinkPara.IKS.Infrastructure;

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
    public static async Task AddApplicationUser(this IServiceCollection services, IVaultClient vaultClient = null)
    {
        services.AddHttpClient<IUserService, UserService>(client =>
        {
            client.BaseAddress = new Uri(vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Identity"));
        });
        var applicationUserService = await services.ConfigureApplicationUser("IKS", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services, configuration);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);
        ConfigureHttpClients(services, vaultClient);

        return services;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, IKSDbContext>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<IIKSService, IKSService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
        services.AddScoped<ICardBinService, CardBinService>();
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("IKSSecrets", "ConnectionStrings", "DefaultConnection");


        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<IKSDbContext>(
                        options => options
                            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(IKSDbContext).Assembly.FullName))
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<IKSDbContext>(
                    options => options
                        .UseNpgsql(connectionString, b => b.EnableRetryOnFailure())
                        .UseSnakeCaseNamingConvention()
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment?.ToLowerInvariant() is "staging" or "production")
        {
            var migrator = services.BuildServiceProvider().GetService<IMigrationConfigurator>();
            migrator.Migrate(connectionString, databaseProvider);
        }
    }

    private static void ConfigureMassTransit(IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var eventBusSettings = new EventBusSettings();
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        if (isLocalConfigurationEnabled)
            configuration.GetSection("LocalConfiguration:EventBusSettings").Bind(eventBusSettings);
        else
            eventBusSettings = vaultClient.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings", null);

        services.AddMassTransit(x =>
        {
            x.AddConsumers(Assembly.GetExecutingAssembly());
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri($"rabbitmq://{eventBusSettings.Host}/"),
                     h =>
                     {
                         h.Username(eventBusSettings.Username);
                         h.Password(eventBusSettings.Password);
                     });

                cfg.ReceiveEndpoint("IKS.IKSTimeoutTransaction", e =>
                {
                    e.ConfigureConsumer<IKSTimeoutTransactionConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("IKS.CreateTerminalResponseCheck", e =>
                {
                    e.ConfigureConsumer<CreateTerminalResponseCheckConsumer>(context);
                });
            });
        });
    }

    private static void ConfigureHttpClients(IServiceCollection services, IVaultClient vaultClient)
    {

        services.AddHttpClient<IMerchantService, MerchantService>(client =>
        {
            services.ConfigureCustomHttpClient(client, vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Pf"));
        });

        services.AddHttpClient<IParameterService, ParameterService>(client =>
        {
            services.ConfigureCustomHttpClient(client, vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "BusinessParameter"));
        });
    }

    private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString)
    {
        string token = services.BuildServiceProvider().GetService<IApplicationUserService>().Token;

        client.BaseAddress = new Uri(uriString);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
}