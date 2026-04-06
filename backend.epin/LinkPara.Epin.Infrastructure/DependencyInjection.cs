using LinkPara.SystemUser;
using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Commons.Models.EventBusConfiguration;
using LinkPara.Epin.Infrastructure.Persistence;
using LinkPara.Epin.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using System.Reflection;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.ContextProvider;
using LinkPara.Cache;
using LinkPara.Epin.Infrastructure.Consumers.CronJobs;
using LinkPara.Epin.Infrastructure.Services.EpinHttpClients;
using LinkPara.HttpProviders.Identity;
using LinkPara.Epin.Infrastructure.Services.Secrets;
using LinkPara.HttpProviders.Emoney;
using LinkPara.Epin.Infrastructure.EMoneyServices;
using LinkPara.SharedModels.Migration;
using LinkPara.Audit;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.UrlModel;

namespace LinkPara.Epin.Infrastructure;

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
        var applicationUserService = await services.ConfigureApplicationUser("Epin", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services, configuration, vaultClient);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);

        return services;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, EpinDbContext>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddTransient<IEmoneyService, EMoneyService>();

        services.AddInternalServices();
        services.AddHttpProviders(vaultClient);
    }

    public static IServiceCollection AddHttpProviders(this IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");

        services.AddHttpClient<IUserService, UserService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Identity);
        });
        services.AddHttpClient<IProvisionService, ProvisionService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Emoney);
        });

        services.AddHttpClient<IEpinHttpClient, PerdigitalHttpClient>();

        return services;
    }
    private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString)
    {
        string token = services.BuildServiceProvider().GetService<IApplicationUserService>().Token;

        client.BaseAddress = new Uri(uriString);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
    public static IServiceCollection AddInternalServices(this IServiceCollection services)
    {
        services.AddScoped<IPublisherService, PublisherService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<IReconciliationService, ReconciliationService>();
        services.AddScoped<IOrderHistoryService, OrderHistoryService>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
        services.AddSingleton<SecretService>();

        return services;
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("EpinSecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<EpinDbContext>(
                        options => options
                            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(EpinDbContext).Assembly.FullName))
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<EpinDbContext>(
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
                cfg.ReceiveEndpoint("Epin.SyncEpinProducts", e =>
                {
                    e.ConfigureConsumer<ProductConsumer>(context);
                }); 
                cfg.ReceiveEndpoint("Epin.CheckEpinOrders", e =>
                {
                    e.ConfigureConsumer<ReconciliationConsumer>(context);
                });
            });
        });
    }
}
