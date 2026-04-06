using LinkPara.Billing.Infrastructure.Persistence;
using LinkPara.Billing.Infrastructure.Services;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.EventBusConfiguration;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LinkPara.HttpProviders.Identity;
using LinkPara.SystemUser;
using LinkPara.Billing.Infrastructure.Services.Billing;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Interfaces;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.Billing.Infrastructure.Services.Mapping;
using LinkPara.Billing.Infrastructure.Services.VendorServices;
using LinkPara.Billing.Infrastructure.Services.SectorServices;
using LinkPara.Billing.Infrastructure.Services.InstitutionServices;
using LinkPara.Billing.Infrastructure.Consumers.CronJobs;
using LinkPara.Audit;
using LinkPara.Billing.Infrastructure.Services.CommissionServices;
using LinkPara.Billing.Infrastructure.Services.FieldServices;
using LinkPara.HttpProviders.Vault;
using LinkPara.Billing.Infrastructure.Services.TransactionServices;
using LinkPara.HttpProviders.Emoney;
using LinkPara.Billing.Infrastructure.Services.SavedBillServices;
using LinkPara.Billing.Infrastructure.Services.Approval;
using LinkPara.Approval;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Interfaces;
using LinkPara.Billing.Infrastructure.Services.BillingServices;
using LinkPara.Billing.Infrastructure.Services.AccountingServices;
using LinkPara.Cache;
using LinkPara.Billing.Infrastructure.Services.ReconciliationServices;
using LinkPara.HttpProviders.Accounting;
using LinkPara.Billing.Infrastructure.Services.EMoneyServices;
using LinkPara.SharedModels.Migration;
using LinkPara.Billing.Infrastructure.Services.IntegrationLoggingServices;
using LinkPara.HttpProviders.Fraud;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.UrlModel;
using LinkPara.Security;

namespace LinkPara.Billing.Infrastructure
{
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
            var applicationUserService = await services.ConfigureApplicationUser("Billing", vaultClient);

            services.AddSingleton<IApplicationUserService>(applicationUserService);
        }
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
        {
            ConfigureServices(services, vaultClient);
            ConfigureDatabase(services, configuration, vaultClient);
            ConfigureMassTransit(services, configuration, vaultClient);

            return services;
        }

        private static void ConfigureServices(IServiceCollection services, IVaultClient vaultClient)
        {

            services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
            services.AddScoped<DbContext, BillingDbContext>();
            services.AddScoped<IDomainEventService, DomainEventService>();
            services.AddScoped<IContextProvider, CurrentContextProvider>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<ICommissionService, CommissionService>();
            services.AddScoped<IApprovalService, ApprovalService>();
            services.AddScoped<IApprovalScreenFactory, BillingApprovalScreenFactory>();
            services.AddScoped<InstitutionScreenService>();
            services.AddScoped<IApprovalScreenService, InstitutionScreenService>();
            services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
            services.AddScoped<IVakifKatilimApi, VakifKatilimApi>();
            services.AddTransient<IIntegrationLogger, IntegrationLoggerService>();
            services.AddTransient<ISecureRandomGenerator, SecureRandomGenerator>();
            services.AddHttpProviders(vaultClient);
            services.AddInternalServices();
            services.AddExternalServices();

        }

        private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration,
            IVaultClient vaultClient)
        {
            var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
            var connectionString =
                isLocalConfigurationEnabled ?
                configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
                : vaultClient.GetSecretValue<string>("BillingSecrets", "ConnectionStrings", "DefaultConnection");

            string databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

            switch (databaseProvider)
            {
                case "MsSql":
                    services
                    .AddDbContext<BillingDbContext>(
                    options => options
                    .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(BillingDbContext).Assembly.FullName))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                    break;
                default:
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                    services.AddDbContext<BillingDbContext>(
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
                    cfg.ReceiveEndpoint("Billing.SyncBillingSectors", e =>
                    {
                        e.ConfigureConsumer<SectorConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("Billing.SyncBillingInstitutions", e =>
                    {
                        e.ConfigureConsumer<InstitutionConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("Billing.BillingReconciliationJob", e =>
                    {
                        e.ConfigureConsumer<ReconciliationConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("Billing.BillingTimeoutJob", e =>
                    {
                        e.ConfigureConsumer<TimeoutConsumer>(context);
                    });
                });
            });
        }

        public static IServiceCollection AddInternalServices(this IServiceCollection services)
        {
            services.AddTransient<IVendorMapper, VendorMapperService>();
            services.AddTransient<IVendorService, VendorService>();
            services.AddTransient<ISectorService, SectorService>();
            services.AddTransient<IInstitutionService, InstitutionService>();
            services.AddTransient<IFieldService, FieldService>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<ISavedBillService, SavedBillService>();
            services.AddTransient<IAccountingService, AccountingService>();
            services.AddTransient<IEmoneyService, EMoneyService>();
            services.AddTransient<IReconciliationService, ReconcilationService>();

            return services;
        }

        public static IServiceCollection AddExternalServices(this IServiceCollection services)
        {
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<ISekerBankApi, SekerBankApi>();
            services.AddScoped<IBillingVendorServiceFactory, BillingServiceFactory>();
            services.AddScoped<SekerBankBillingService>()
                .AddScoped<IBillingVendorService, SekerBankBillingService>();
            services.AddScoped<MockBillingService>()
                .AddScoped<IBillingVendorService, MockBillingService>();
            services.AddScoped<VakifKatilimBillingService>()
                .AddScoped<IBillingVendorService, VakifKatilimBillingService>();
            return services;
        }

        public static IServiceCollection AddHttpProviders(this IServiceCollection services, IVaultClient vaultClient)
        {
            var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");

            services.AddHttpClient<IParameterService, ParameterService>(client =>
            {
                services.ConfigureCustomHttpClient(client, serviceUrls.BusinessParameter);
            });
            services.AddHttpClient<IProvisionService, ProvisionService>(client =>
            {
                services.ConfigureCustomHttpClient(client, serviceUrls.Emoney);
            });
            services.AddHttpClient<IPaymentService, PaymentService>(client =>
            {
                services.ConfigureCustomHttpClient(client, serviceUrls.Accounting);
            });
            services.AddHttpClient<IFraudTransactionService, FraudTransactionService>(client =>
            {
                services.ConfigureCustomHttpClient(client, serviceUrls.Fraud);
            });
            services.AddHttpClient<IAccountService, AccountService>(client =>
            {
                services.ConfigureCustomHttpClient(client, serviceUrls.Emoney);
            });

            return services;
        }
        private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString)
        {
            string token = services.BuildServiceProvider().GetService<IApplicationUserService>().Token;

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
    }
}

