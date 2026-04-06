using System.Reflection;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.Scheduler.API.Commons.Models;
using LinkPara.Scheduler.API.Persistence;
using LinkPara.Scheduler.API.Services;
using LinkPara.SharedModels.Migration;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using LinkPara.HttpProviders.Identity;
using LinkPara.SystemUser;

namespace LinkPara.Scheduler.API;

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
        var applicationUserService = await services.ConfigureApplicationUser("Scheduler", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureHangfireScheduler(services, configuration, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);

        return services;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IJobScheduler, JobSchedulerService>();
        services.AddHttpClient<IJobHttpInvoker, JobHttpInvoker>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, SchedulerDbContext>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
    }
    
    private static void ConfigureHangfireScheduler(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        
        var connectionString = isLocalConfigurationEnabled 
            ? configuration.GetValue<string>("LocalConfiguration:DefaultConnection") 
            : vaultClient.GetSecretValue<string>("SchedulerSecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");
        

        switch (databaseProvider)
        {
            case "MsSql":
                var sqlServerStorageOptions = new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                    SchemaName = "Core"
                };
                // Add Hangfire services.
                services.AddHangfire(conf => conf
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString, sqlServerStorageOptions));
                JobStorage.Current = new SqlServerStorage(connectionString, sqlServerStorageOptions);
                break;
            default:
                var postgreServerStorageOptions = new PostgreSqlStorageOptions
                {
                    SchemaName = "core"
                };
                services.AddHangfire(conf => conf
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(connectionString, postgreServerStorageOptions));
                JobStorage.Current = new PostgreSqlStorage(connectionString, postgreServerStorageOptions);
                break;
        }
        
        // Add the processing server as IHostedService
        services.AddHangfireServer();
    }

    private static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("SchedulerSecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");
        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<SchedulerDbContext>(
                        options => options
                            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(SchedulerDbContext).Assembly.FullName))
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<SchedulerDbContext>(
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
    
    private static void ConfigureMassTransit(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var eventBusSettings = new EventBusSetting();
        if (isLocalConfigurationEnabled)
        {
            configuration.GetSection("LocalConfiguration:EventBusSettings").Bind(eventBusSettings);
        }
        else
        {
            eventBusSettings = vaultClient.GetSecretValue<EventBusSetting>("SharedSecrets", "EventBusSettings", null);
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
            });
        });
    }
    
}
