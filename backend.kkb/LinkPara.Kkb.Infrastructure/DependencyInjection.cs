using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.Kkb.Application.Commons.Interfaces;
using LinkPara.Kkb.Application.Commons.Models.EventBusConfiguration;
using LinkPara.Kkb.Infrastructure.Persistence;
using LinkPara.HttpProviders.Vault;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LinkPara.Kkb.Infrastructure.ExternalServices.Kkb;
using LinkPara.Kkb.Application.Commons.Models.KkbSettings;
using LinkPara.Kkb.Infrastructure.Services;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.HttpProviders.Identity;
using LinkPara.SharedModels.Migration;
using LinkPara.SharedModels.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.Audit;

namespace LinkPara.Kkb.Infrastructure;

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
        var applicationUserService = await services.ConfigureApplicationUser("Kkb", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);
        ConfigureHttpClients(services, vaultClient);

        return services;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, KkbDbContext>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<IKkbValidationService, KkbValidationService>(); 
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
        services.AddScoped<IAuditLogService, AuditLogService>();
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration,
        IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("KkbSecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<KkbDbContext>(
                        options => options
                            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(KkbDbContext).Assembly.FullName))
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<KkbDbContext>(
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

    private static void ConfigureHttpClients(IServiceCollection services, IVaultClient vaultClient)
    {
        var kkbApiAuthorizationSettings = vaultClient.GetSecretValue<KkbApiAuthorizationSettings>("KkbSecrets", "KkbApiAuthorization");
        var businessParameterServiceUrl = vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "BusinessParameter");
        services.AddSingleton(settings => kkbApiAuthorizationSettings);
        services.AddHttpClient<KkbAuthorizationService, KkbAuthorizationService>();
        services.AddHttpClient<IParameterService, ParameterService>(client =>
        {
            services.ConfigureCustomHttpClient(client, businessParameterServiceUrl);
        });
    }

    private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString)
    {
        string token = services.BuildServiceProvider().GetService<IApplicationUserService>().Token;
        client.BaseAddress = new Uri(uriString);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    private static void ConfigureMassTransit(IServiceCollection services, IConfiguration configuration,
        IVaultClient vaultClient)
    {
        var eventBusSettings = new EventBusSettings();
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        if (isLocalConfigurationEnabled)
            configuration.GetSection("LocalConfiguration:EventBusSettings").Bind(eventBusSettings);
        else eventBusSettings = vaultClient.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings", null);

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
