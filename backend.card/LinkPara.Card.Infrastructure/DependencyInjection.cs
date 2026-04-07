using LinkPara.Audit;
using LinkPara.Cache;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.EventBusConfiguration;
using LinkPara.Card.Application.Features.PaycoreServices.CardPinServices.Services;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services;
using LinkPara.Card.Infrastructure.Services.PaycoreServices;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Services;
using LinkPara.Card.Infrastructure.Services.WalletServices.Services;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.DataContainer;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Migration;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.UrlModel;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LinkPara.Card.Infrastructure;

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
        var applicationUserService = await services.ConfigureApplicationUser("Card", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureHttpClients(services, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);

        return services;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, CardDbContext>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<PaycoreClientService>();
        services.AddScoped<IPaycoreCardService, PaycoreCardService>();
        services.AddScoped<IPaycoreCustomerService, PaycoreCustomerService>();
        services.AddScoped<IPaycoreDebitAuthorizationService, DebitAuthorizationService>();
        services.AddScoped<IPaycoreSecurityService, PaycoreSecurityService>();
        services.AddScoped<IPaycoreParameterService, PaycoreParameterService>();
        services.AddScoped<IPinBlockService, PinBlockService>();
        services.AddScoped<ICustomerWalletCardService, CustomerWalletCardService>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
        services.AddScoped<IDbCommandInterceptor, LongQueryLogger>();
        services.AddScoped<ISecureRandomGenerator, SecureRandomGenerator>();
    }

    private static void ConfigureHttpClients(IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");

        var forwardToken = services.BuildServiceProvider()
            .GetService<IApplicationUserService>().Token;

        services.AddHttpClient<IDataContainerProvider, DataContainerProviderService>(client =>
        {
            services.ConfigureCustomHttpClient(client, string.Concat(serviceUrls.Content, "/v1/DataContainers/"), forwardToken);
        });

        services.AddHttpClient<IWalletService, WalletService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Emoney, forwardToken);
        }); 
        
        services.AddHttpClient<ICustomerTransactionService, CustomerTransactionService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Emoney, forwardToken);
        });

    }
    private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString, string forwardToken)
    {
        client.BaseAddress = new Uri(uriString);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {forwardToken}");
        client.DefaultRequestHeaders.Add("Gateway", Gateway.BoaApiGateway.ToString()); //todo: remove 
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration,
        IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("CardSecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<CardDbContext>(
                        options => options
                            .UseSqlServer(connectionString, b => b
                                .MigrationsAssembly(typeof(CardDbContext).Assembly.FullName)
                                .EnableRetryOnFailure())
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<CardDbContext>((sp, options) => options
                        .UseNpgsql(connectionString, b => b.EnableRetryOnFailure())
                        .UseSnakeCaseNamingConvention()
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        .AddInterceptors(sp.GetRequiredService<IDbCommandInterceptor>()));
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
