using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Commons.Models.EventBusConfiguration;
using LinkPara.Cache;
using LinkPara.BusinessParameter.Infrastructure.Persistence;
using LinkPara.BusinessParameter.Infrastructure.Services;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using System.Reflection;
using LinkPara.Audit;
using LinkPara.HttpProviders.Vault;
using LinkPara.Approval;
using LinkPara.BusinessParameter.Infrastructure.Services.Approval;
using LinkPara.SharedModels.Migration;
using LinkPara.HttpProviders.Identity;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.BusinessParameter.Infrastructure;

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
        var applicationUserService = await services.ConfigureApplicationUser("BusinessParameter", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);

        return services;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, BusinessParameterDbContext>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<IParameterService, ParameterService>();
        services.AddScoped<IParameterGroupService, ParameterGroupService>();
        services.AddScoped<IParameterTemplateService, ParameterTemplateService>();
        services.AddScoped<IParameterTemplateValueService, ParameterTemplateValueService>();
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
                : vaultClient.GetSecretValue<string>("BusinessParameterSecrets", "ConnectionStrings", "DefaultConnection");

        string databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                .AddDbContext<BusinessParameterDbContext>(
                options => options
                .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(BusinessParameterDbContext).Assembly.FullName))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<BusinessParameterDbContext>(
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

    private static void ConfigureMassTransit(IServiceCollection services, IConfiguration configuration,
        IVaultClient vaultClient)
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
            });
        });
    }

    public static void AddApprovalScreenService(this IServiceCollection services)
    {
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IApprovalScreenFactory, BusinessParameterApprovalScreenFactory>();

        services.AddScoped<ParameterGroupScreenService>()
                .AddScoped<IApprovalScreenService, ParameterGroupScreenService>();
        services.AddScoped<ParameterTemplateScreenService>()
                .AddScoped<IApprovalScreenService, ParameterTemplateScreenService>();
        services.AddScoped<ParameterScreenService>()
                .AddScoped<IApprovalScreenService, ParameterScreenService>();
    }
}
