using System.Reflection;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.LogConsumers.Commons.EventBusConfiguration;
using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.LogConsumers.Consumers;
using LinkPara.LogConsumers.Persistence;
using LinkPara.LogConsumers.Services;
using LinkPara.SharedModels.Migration;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.SharedModels.UrlModel;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;

namespace LinkPara.LogConsumers;

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
        services.AddHttpContextAccessor();
        services.AddHttpClient<IUserService, UserService>(client =>
        {
            client.BaseAddress = new Uri(vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Identity"));
        });
        var applicationUserService = await services.ConfigureApplicationUser("LogConsumer", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services);
        ConfigureHttpClients(services, vaultClient);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);

        return services;
    }

    private static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IElasticSearchService, ElasticSearchService>();
        services.AddSingleton<IConfidentialService, ConfidentialService>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, LogConsumerDbContext>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
    }

    private static void ConfigureHttpClients(IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");
        var forwardToken = services.BuildServiceProvider().GetService<IApplicationUserService>().Token;

        services.AddHttpClient<IUserService, UserService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Identity, forwardToken);
        });

        services.AddHttpClient<IParameterService, ParameterService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.BusinessParameter, forwardToken);
        });
    }

    private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString, string forwardToken)
    {
        client.BaseAddress = new Uri(uriString);
        client.DefaultRequestHeaders.Add("Authorization", (string)$"Bearer {forwardToken}");
    }

    private static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("LogConsumerSecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");
        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<LogConsumerDbContext>(
                        options => options
                            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(LogConsumerDbContext).Assembly.FullName))
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<LogConsumerDbContext>(
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
            x.AddConsumers(Assembly.GetExecutingAssembly());
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri($"rabbitmq://{eventBusSettings.Host}/"),
                     h =>
                     {
                         h.Username(eventBusSettings.Username);
                         h.Password(eventBusSettings.Password);
                     });

                cfg.ReceiveEndpoint("Log.RequestResponseCreated", e =>
                     {
                         e.ConfigureConsumer<RequestResponseLogCreatedConsumer>(context);
                     });
                cfg.ReceiveEndpoint("Log.UserLogin", e =>
                    {
                        e.ConfigureConsumer<UserLoginLogConsumer>(context);
                    });
                cfg.ReceiveEndpoint("Log.AuditLog", e =>
                {
                    e.ConfigureConsumer<AuditLogConsumer>(context);
                });
                cfg.ReceiveEndpoint("Log.UserActivityLog", e =>
                {
                    e.ConfigureConsumer<UserActivityLogConsumer>(context);
                });
                cfg.ReceiveEndpoint("Log.ChangeTracker", e =>
                {
                    e.ConfigureConsumer<ChangeTrackerConsumer>(context);
                });
                cfg.ReceiveEndpoint("Log.BankAccountActivity", e =>
                {
                    e.ConfigureConsumer<BankAccountActivityConsumer>(context);
                });
                cfg.ReceiveEndpoint("Log.IntegrationLog", e =>
                {
                    e.ConfigureConsumer<IntegrationLogConsumer>(context);
                });
                cfg.ReceiveEndpoint("Log.LongQueryLog", e =>
                {
                    e.ConfigureConsumer<LongQueryLogConsumer>(context);
                });
                cfg.ReceiveEndpoint("Log.ExceptionLog", e =>
                {
                    e.UseRawJsonDeserializer(isDefault: true);
                    e.ConfigureConsumer<ExceptionLogConsumer>(context);
                    e.PrefetchCount = 500;
                    e.Batch<ExceptionLog>(b =>
                    {
                        b.MessageLimit = 100;
                        b.ConcurrencyLimit = 3;
                        b.TimeLimit = TimeSpan.FromSeconds(10.0);
                    });
                });
            });
        });

        return services;
    }
}
