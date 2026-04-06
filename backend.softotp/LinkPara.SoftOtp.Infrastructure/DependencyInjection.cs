using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LinkPara.Cache;
using LinkPara.SharedModels.Migration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SoftOtp.Application.Common.Models;
using LinkPara.SharedModels.Persistence;
using LinkPara.SoftOtp.Infrastructure.Integration;
using LinkPara.SoftOtp.Infrastructure.Persistence;
using LinkPara.SoftOtp.Infrastructure.Services;
using LinkPara.Audit;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.SharedModels.UrlModel;

namespace LinkPara.SoftOtp.Infrastructure;

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
        var applicationUserService = await services.ConfigureApplicationUser("SoftOTP", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services);
        ConfigureMassTransit(services, configuration, vaultClient);
        ConfigureHttpClients(services, vaultClient);
        return services;
    }
    
    private static void ConfigureHttpClients(IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");
        var forwardToken = services.BuildServiceProvider().GetService<IApplicationUserService>().Token;
        
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
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<IMultifactorService, MultifactorService>();
        services.AddScoped<ISmsSender, SmsSenderService>();
        services.AddScoped<IPowerFactorAdapter, PowerFactorAdapter>();
        services.AddScoped<IPushNotificationSender, PushNotificationSenderService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddTransient<IIntegrationLogger, IntegrationLoggerService>();
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration,
        IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");

        string connectionString = isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("SoftOTPSecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<ApplicationDbContext>(
                        options => options
                    .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<ApplicationDbContext>(
                        options => options
                    .UseNpgsql(connectionString, b => b.EnableRetryOnFailure())
                    .UseSnakeCaseNamingConvention()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environment?.ToLowerInvariant() is "staging" or "production")
        {
            var migrator = services.BuildServiceProvider()
                .GetService<IMigrationConfigurator>();

            migrator.Migrate(connectionString, databaseProvider);
        }
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
