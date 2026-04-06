using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.Accounting.Application.Commons.Models.EventBusConfiguration;
using LinkPara.Accounting.Infrastructure.Consumers;
using LinkPara.Accounting.Infrastructure.Persistence;
using LinkPara.Accounting.Infrastructure.Services;
using LinkPara.Audit;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Migration;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LinkPara.SharedModels.Persistence;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.Accounting.Infrastructure.Services.AccountingServices;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.Security;
using LinkPara.Approval;
using LinkPara.Accounting.Infrastructure.Services.Approval;

namespace LinkPara.Accounting.Infrastructure;

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
        var applicationUserService = await services.ConfigureApplicationUser("Accounting", vaultClient);

        services.AddSingleton(applicationUserService);
    }
    private static void ConfigureHttpClients(IServiceCollection services, IVaultClient vaultClient)
    {
        var forwardToken = services.BuildServiceProvider().GetService<IApplicationUserService>().Token;

        services.AddHttpClient<IParameterService, ParameterService>(client =>
        {
            services.ConfigureCustomHttpClient(client, vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "BusinessParameter"), forwardToken);
        });
    }

    private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString, string forwardToken)
    {
        client.BaseAddress = new Uri(uriString);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {forwardToken}");
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
    IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services, vaultClient);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);
        ConfigureHttpClients(services, vaultClient);

        return services;
    }

    private static void ConfigureServices(IServiceCollection services, IVaultClient vaultClient)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, AccountingDbContext>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<ISecureRandomGenerator, SecureRandomGenerator>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IApprovalScreenFactory, AccountingApprovalScreenFactory>();
        services.AddScoped<DefaultScreenService>();
        services.AddScoped<IApprovalScreenService, DefaultScreenService>();
        services.AddScoped<PaymentScreenService>();
        services.AddScoped<IApprovalScreenService, PaymentScreenService>();
        services.AddScoped<CustomerScreenService>();
        services.AddScoped<IApprovalScreenService, CustomerScreenService>();


        var provider = vaultClient.GetSecretValue<string>("AccountingSecrets", "LogoProvider", "Provider");

        if (!string.IsNullOrEmpty(provider) && provider == "Alternatif")
        {
            services.AddHttpClient<IAccountingService, AlternatifAccountingService>(client =>
            {
                var uri = vaultClient.GetSecretValue<string>("AccountingSecrets", "AlternatifLogoSettings", "ServiceUri");
                client.BaseAddress = new Uri(uri);
            });

            services.AddHttpClient<IInvoiceService, AlternatifInvoiceService>(client =>
            {
                var uri = vaultClient.GetSecretValue<string>("AccountingSecrets", "AlternatifLogoSettings", "ServiceUri");
                client.BaseAddress = new Uri(uri);
            });
        }
        else
        {
            services.AddHttpClient<IAccountingService, TigerAccountingService>(client =>
            {
                var uri = vaultClient.GetSecretValue<string>("AccountingSecrets", "LogoSettings", "ServiceUri");
                client.BaseAddress = new Uri(uri);
            });
            services.AddHttpClient<IInvoiceService, TigerInvoiceService>(client =>
            {
                var uri = vaultClient.GetSecretValue<string>("AccountingSecrets", "LogoSettings", "ServiceUri");
                client.BaseAddress = new Uri(uri);
            });
        }

    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("AccountingSecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<AccountingDbContext>(
                        options => options
                    .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AccountingDbContext).Assembly.FullName))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<AccountingDbContext>(
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
                cfg.ReceiveEndpoint("Accounting.CreateCustomer", e =>
                {
                    e.ConfigureConsumer<CreateAccountingCustomerConsumer>(context);
                });
                cfg.ReceiveEndpoint("Accounting.SavePayment", e =>
                {
                    e.ConfigureConsumer<SavePaymentConsumer>(context);
                });
                cfg.ReceiveEndpoint("Accounting.RetryFailedPayment", e =>
                {
                    e.ConfigureConsumer<RetryFailedPaymentConsumer>(context);
                });
                cfg.ReceiveEndpoint("Accounting.ProcessCustomerInvoices", e =>
                {
                    e.ConfigureConsumer<EmoneyProcessInvoicesConsumer>(context);
                });
                cfg.ReceiveEndpoint("Accounting.UpdateCustomer", e =>
                {
                    e.ConfigureConsumer<UpdateAccountingCustomerConsumer>(context);
                });
            });
        });
    }
}
