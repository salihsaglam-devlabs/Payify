using LinkPara.Audit;
using LinkPara.Cache;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Helpers;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Models.EventBusConfiguration;
using LinkPara.Card.Application.Commons.Models.Locking;
using LinkPara.Card.Application.Commons.Models.Scheduler;
using LinkPara.Card.Infrastructure.Consumers.CronJobs;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.EMoneyServices;
using LinkPara.Card.Infrastructure.Services;
using LinkPara.Card.Infrastructure.Services.FileIngestion.Parsers;
using LinkPara.Card.Infrastructure.Services.FileIngestion.RemoteFiles;
using LinkPara.Card.Infrastructure.Services.FileIngestion.Orchestration;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Engine;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Engine.Operations;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration;
using LinkPara.Card.Infrastructure.Services.Logging;
using LinkPara.Card.Infrastructure.Services.Notifications;
using LinkPara.Card.Infrastructure.Services.PaycoreServices;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Services;
using LinkPara.Card.Infrastructure.Services.Concurrency;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.DataContainer;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
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
        ConfigureFileIngestionAndReconciliationOptions(services, vaultClient);
        ConfigureServices(services);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureHttpClients(services, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);

        return services;
    }

    private static void ConfigureFileIngestionAndReconciliationOptions(
        IServiceCollection services,
        IVaultClient vaultClient)
    {
        var vaultSettings = vaultClient.GetSecretValue<FileIngestionAndReconciliationVaultSettings>("CardSecrets", "FileIngestionAndReconciliationSettings", null);
        if (vaultSettings is null)
        {
            throw new InvalidOperationException("Vault key missing: CardSecrets/FileIngestionAndReconciliationSettings");
        }

        services.Configure<FileIngestionSettings>(options =>
        {
            var value = vaultSettings.FileIngestion ?? new FileIngestionSettings();
            options.Ftp = value.Ftp ?? new FtpIngestionSettings();
            options.Local = value.Local ?? new LocalIngestionSettings();
            options.TimestampFormat = string.IsNullOrWhiteSpace(value.TimestampFormat) ? "yyyyMMdd_HHmmss" : value.TimestampFormat;
            options.MaxFilesPerRun = value.MaxFilesPerRun;
            options.FileEncoding = string.IsNullOrWhiteSpace(value.FileEncoding) ? "UTF-8" : value.FileEncoding;
            options.FileDetection = value.FileDetection ?? new FileDetectionSettings();
            options.ReconciliationProcessing = value.ReconciliationProcessing ?? new ReconciliationProcessingSettings();
            options.Alarm = value.Alarm ?? new ReconciliationAlarmSettings();
        });

        services.Configure<FileParsingRulesOptions>(options =>
        {
            var value = vaultSettings.FileParsingRules ?? new FileParsingRulesOptions();
            options.Files = value.Files ?? new Dictionary<string, FixedWidthFileRule>(StringComparer.OrdinalIgnoreCase);
        });

        services.Configure<ProcessExecutionLockSettings>(options =>
        {
            var value = vaultSettings.ProcessExecutionLockSettings ?? new ProcessExecutionLockSettings();
            options.Enabled = value.Enabled;
            options.Acquire = value.Acquire ?? new ProcessExecutionLockAcquireSettings();
            options.Renewal = value.Renewal ?? new ProcessExecutionLockRenewalSettings();
            options.Cleanup = value.Cleanup ?? new ProcessExecutionLockCleanupSettings();
            options.Policies = value.Policies is { Count: > 0 }
                ? new Dictionary<string, ProcessExecutionLockPolicySettings>(value.Policies, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, ProcessExecutionLockPolicySettings>(new ProcessExecutionLockSettings().Policies, StringComparer.OrdinalIgnoreCase);
        });
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, CardDbContext>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<INotificationEmailService, NotificationEmailService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IDbCommandInterceptor, LongQueryLogger>();
        services.AddScoped<PaycoreClientService>();
        services.AddScoped<IPaycoreCardService, PaycoreCardService>();
        services.AddScoped<IPaycoreCustomerService, PaycoreCustomerService>();
        services.AddScoped<IPaycoreSecurityService, PaycoreSecurityService>();
        services.AddScoped<IBulkServiceOperationLogPublisher, BulkServiceOperationLogPublisher>();
        services.AddScoped<IFileIngestionService, FileIngestionService>();
        services.AddScoped<IReconciliationService, ReconciliationService>();
        services.AddScoped<IReconciliationAlarmService, ReconciliationAlarmService>();
        services.AddScoped<IReconciliationManualOperationService, ReconciliationManualOperationService>();
        services.AddScoped<IReconciliationOperationHandler, AdjustResponseCodeOperationHandler>();
        services.AddScoped<IReconciliationOperationHandler, ReverseBalanceOperationHandler>();
        services.AddScoped<IReconciliationOperationHandler, SetExpireStatusOperationHandler>();
        services.AddScoped<IReconciliationOperationHandler, CreateTransactionOperationHandler>();
        services.AddScoped<IReconciliationOperationHandler, ApplyRefundToOriginalOperationHandler>();
        services.AddScoped<IReconciliationOperationHandler, ApplyManualRefundIfApprovedOperationHandler>();
        services.AddScoped<IReconciliationOperationHandler, MarkOriginalCancelledOperationHandler>();
        services.AddScoped<IReconciliationOperationHandler, ApplyReversalOperationHandler>();
        services.AddScoped<IReconciliationOperationHandler, ApplyShadowBalanceOperationHandler>();
        services.AddScoped<IReconciliationOperationHandler, QueueBalanceFixListOperationHandler>();
        services.AddScoped<IReconciliationOperationExecutor, ReconciliationOperationExecutor>();
        services.AddScoped<IReconciliationAutoOperationService, ReconciliationAutoOperationService>();
        services.AddScoped<IProcessExecutionLockService, ProcessExecutionLockService>();
        services.AddScoped<IFileParser, CardTransactionsFileParser>();
        services.AddScoped<IFileParser, ClearingTransactionsFileParser>();
        services.AddScoped<RemoteFileClient>();
        services.AddScoped<IRemoteFileReader>(provider => provider.GetRequiredService<RemoteFileClient>());
        services.AddScoped<IRemoteFileWriter>(provider => provider.GetRequiredService<RemoteFileClient>());
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

        services.AddHttpClient<IEMoneyService, EMoneyService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

    }

    private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString, string forwardToken)
    {
        client.BaseAddress = new Uri(uriString);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {forwardToken}");
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
                cfg.ReceiveEndpoint(CronJobEndpointNames.SchedulerSerialCardJobs, e =>
                {
                    e.PrefetchCount = 1;
                    e.ConcurrentMessageLimit = 1;
                    e.SetQueueArgument("x-single-active-consumer", true);
                    e.UseRawJsonDeserializer(isDefault: true);
                    e.ConfigureConsumer<FileIngestionAndReconciliationConsumer>(context);
                });
            });
        });
    }
}
