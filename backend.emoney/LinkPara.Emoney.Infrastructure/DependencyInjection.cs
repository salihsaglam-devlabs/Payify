using LinkPara.Approval;
using LinkPara.Audit;
using LinkPara.Backend.Emoney.PaymentProvider.Commons;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.EventBusConfiguration;
using LinkPara.Emoney.Infrastructure.Consumers;
using LinkPara.Emoney.Infrastructure.Consumers.CronJobs;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.Emoney.Infrastructure.Services;
using LinkPara.Emoney.Infrastructure.Services.Approval;
using LinkPara.Emoney.Infrastructure.Services.Limit;
using LinkPara.Emoney.Infrastructure.Services.Masterpass;
using LinkPara.Emoney.Infrastructure.Services.PaymentProvider;
using LinkPara.Emoney.Infrastructure.Services.PaymentProvider.PayifyPf;
using LinkPara.Emoney.Infrastructure.Services.Secrets;
using LinkPara.HttpProviders.Approval;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Calendar;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.DataContainer;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.HttpProviders.Documents;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.KKB;
using LinkPara.HttpProviders.KPS;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MultiFactor;
using LinkPara.HttpProviders.Notification;
using LinkPara.HttpProviders.PF;
using LinkPara.HttpProviders.Receipt;
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

namespace LinkPara.Emoney.Infrastructure;

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
        var applicationUserService = await services.ConfigureApplicationUser("Emoney", vaultClient);

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
        services.AddScoped<DbContext, EmoneyDbContext>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IWalletNumberGenerator, WalletNumberGenerator>();
        services.AddScoped<IBankService, BankService>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<IPricingProfileService, PricingProfileService>();
        services.AddScoped<ILimitService, LimitService>();
        services.AddScoped<ITierLevelService, TierLevelService>();
        services.AddScoped<ITierPermissionService, TierPermissionService>();
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IUserActivityLogService, UserActivityLogService>();
        services.AddScoped<IEmailSender, EmailSenderService>();
        services.AddScoped<IPushNotificationSender, PushNotificationSenderService>();
        services.AddScoped<IWithdrawRequestService, WithdrawRequestService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IApprovalScreenFactory, EmoneyApprovalScreenFactory>();
        services.AddScoped<LimitScreenService>();
        services.AddScoped<IApprovalScreenService, LimitScreenService>();
        services.AddScoped<IProvisionService, ProvisionService>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
        services.AddScoped<IBTransService, BTransService>();
        services.AddSingleton<IParameterService, ParameterService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPricingCommercialService, PricingCommercialService>();
        services.AddScoped<IAccountIbanService, AccountIbanService>();
        services.AddScoped<IKpsService, KpsService>();
        services.AddScoped<IAccountActivityService, AccountActivityService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IDbCommandInterceptor, LongQueryLogger>();
        services.AddScoped<ISecureRandomGenerator, SecureRandomGenerator>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IHashGenerator, HashGenerator>();
        services.AddScoped<ISecureKeyGenerator, SecureKeyGenerator>();
        services.AddScoped<IVirtualIbanService, VirtualIbanService>();
        services.AddScoped<ISignatureGenerator, SignatureGenerator>();
        services.AddScoped<IPaymentApiLog, PfPaymentApiLog>();
        services.AddScoped<ITopupService, TopupService>();
        services.AddScoped<IPaymentProviderServiceFactory, PaymentProviderServiceFactory>();
        services.AddScoped<PayifyPfService>()
                .AddScoped<IPaymentProviderService, PayifyPfService>();
        services.AddScoped<IMasterpassService, MasterpassService>();
        services.AddScoped<CustodyAccountScreenService>();
        services.AddScoped<IApprovalScreenService, CustodyAccountScreenService>();
        services.AddScoped<DefaultScreenService>();
        services.AddScoped<IApprovalScreenService, DefaultScreenService>();
        services.AddScoped<CommercialPricingScreenService>();
        services.AddScoped<IApprovalScreenService, CommercialPricingScreenService>();
        services.AddScoped<PricingProfileScreenService>();
        services.AddScoped<IApprovalScreenService, PricingProfileScreenService>();
        services.AddScoped<TierLevelScreenService>();
        services.AddScoped<IApprovalScreenService, TierLevelScreenService>();
        services.AddScoped<TierPermissionScreenService>();
        services.AddScoped<IApprovalScreenService, TierPermissionScreenService>();
        services.AddScoped<TopupScreenService>();
        services.AddScoped<IApprovalScreenService, TopupScreenService>();
        services.AddScoped<WalletScreenService>();
        services.AddScoped<IApprovalScreenService, WalletScreenService>();
        services.AddScoped<IIbanBlacklistService, IbanBlacklistService>();
        services.AddScoped<IBulkTransferService, BulkTransferService>();
        services.AddScoped<ICorporateWalletService, CorporateWalletService>();
        services.AddScoped<IOnUsPaymentService, OnUsPaymentService>();
        services.AddScoped<IChargebackService, ChargebackService>();
        services.AddScoped<ChargebackScreenService>();
        services.AddScoped<IApprovalScreenService, ChargebackScreenService>();
        services.AddScoped<IOpenBankingService, OpenBankingService>();
        services.AddSingleton<ISecretService, SecretService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IManualTransferService, ManualTransferService>();
        services.AddScoped<ISaveReceiptService, SaveReceiptService>();
        services.AddScoped<IDatabaseProviderService, DatabaseProviderService>();
        services.AddScoped<IPayWithWalletService, PayWithWalletService>();
        services.AddScoped<ICashbackService, CashbackService>();
        services.AddScoped<IUpdateBalanceService, UpdateBalanceService>();
        services.AddScoped<IWalletBlockageService, WalletBlockageService>();
        
        services.AddScoped<IOpenBankingOperationsService, OpenBankingOperationsService>()
            .AddScoped<OpenBankingOperationsService>();

        services.AddScoped<IConsentOperationsService, ConsentOperationsService>()
            .AddScoped<ConsentOperationsService>();
    }

    private static void ConfigureHttpClients(IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");

        var forwardToken = services.BuildServiceProvider()
            .GetService<IApplicationUserService>().Token;

        services.AddHttpClient<IMoneyTransferService, MoneyTransferService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.MoneyTransfer, forwardToken);
        });

        services.AddHttpClient<IDataContainerProvider, DataContainerProviderService>(client =>
        {
            services.ConfigureCustomHttpClient(client, string.Concat(serviceUrls.Content, "/v1/DataContainers/"), forwardToken);
        });

        services.AddHttpClient<ICalendarService, CalendarService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Calendar, forwardToken);
        });

        services.AddHttpClient<IUserService, UserService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Identity, forwardToken);
        });
        services.AddHttpClient<IKKBService, KKBService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.KKB, forwardToken);
        });
        services.AddHttpClient<INotificationService, NotificationService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Notification, forwardToken);
        });
        services.AddHttpClient<IParameterService, ParameterService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.BusinessParameter, forwardToken);
        });

        services.AddHttpClient<IFraudTransactionService, FraudTransactionService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Fraud, forwardToken);
        });

        services.AddHttpClient<IBankApiService, BankApiService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.MoneyTransfer, forwardToken);
        });

        services.AddHttpClient<ICustomerService, CustomerService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.CustomerManagement, forwardToken);
        });

        services.AddHttpClient<IKpsService, KpsService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.KPS, forwardToken);
        });

        services.AddHttpClient<ISearchService, SearchService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Fraud, forwardToken);
        });

        services.AddHttpClient<IMultiFactorService, MultiFactorService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.SoftOtp, forwardToken);
        });

        services.AddHttpClient<IDocumentService, DocumentService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Document, forwardToken);
        });
        services.AddHttpClient<IPfOnUsService, PfOnUsService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Pf, forwardToken);
        });
        services.AddHttpClient<IBankAccountService, BankAccountService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Pf, forwardToken);
        });
        services.AddHttpClient<IReceiptService, ReceiptService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Receipt, forwardToken);
        });
        services.AddHttpClient<IRequestsService, RequestsService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Approval, forwardToken);
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
            : vaultClient.GetSecretValue<string>("EmoneySecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<EmoneyDbContext>(
                        options => options
                            .UseSqlServer(connectionString, b => b
                                .MigrationsAssembly(typeof(EmoneyDbContext).Assembly.FullName)
                                .EnableRetryOnFailure())
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<EmoneyDbContext>((sp, options) => options
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

                cfg.ReceiveEndpoint("Emoney.CheckPendingWithdrawRequest",
                   e => { e.ConfigureConsumer<CheckPendingWithdrawRequestConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.CheckPendingReturnRequest",
                   e => { e.ConfigureConsumer<CheckPendingReturnRequestConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.TransferSaved",
                   e => { e.ConfigureConsumer<TransferSavedConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.TransferReturned",
                    e => { e.ConfigureConsumer<TransferReturnedConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.TransferFailed",
                    e => { e.ConfigureConsumer<TransferFailedConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.TransferCompleted",
                    e => { e.ConfigureConsumer<TransferCompletedConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.ProcessIncomingTransaction",
                    e =>
                    {
                        e.ConfigureConsumer<ProcessIncomingTransactionConsumer>(context);
                        e.UseMessageRetry(s =>
                        {
                            s.Handle<EntityLockedException>();
                            s.Interval(10, 1000);
                        });
                    });

                cfg.ReceiveEndpoint("Emoney.RedeliverWithdrawRequest",
                    e => { e.ConfigureConsumer<RedeliverWithdrawRequestConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.CheckDailyTransferOrders",
                    e => { e.ConfigureConsumer<CheckDailyTransferOrdersConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.CheckPricingProfileStatus",
                    e => { e.ConfigureConsumer<CheckPricingProfileStatusConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.ReturnTransferCompleted",
                    e => { e.ConfigureConsumer<ReturnTransferCompletedConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.SendBalanceInformationReport",
                    e => { e.ConfigureConsumer<SendBalanceInformationReportConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.CreateAccountCustomer",
                    e => { e.ConfigureConsumer<CreateAccountCustomerConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.ResetCommercialAccounts",
                    e => { e.ConfigureConsumer<ResetCommercialAccountConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.CheckCommercialPricingActivationDate",
                    e => { e.ConfigureConsumer<CheckCommercialPricingActivationDateJob>(context); });

                cfg.ReceiveEndpoint("Emoney.CheckVirtualIbanCount",
                    e => { e.ConfigureConsumer<CheckVirtualIbanCountConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.AssignVirtualIban",
                    e => { e.ConfigureConsumer<AssignVirtualIbanConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.TransformCustodyAccounts", e =>
                {
                    e.ConfigureConsumer<TransformCustodyAccountsConsumer>(context);
                });

                cfg.ReceiveEndpoint("Emoney.CheckWithdrawBulkTransfer",
                    e => { e.ConfigureConsumer<CheckWithdrawBulkTransferConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.TopupProvisionTimeout",
                    e => { e.ConfigureConsumer<TopupProvisionTimeoutConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.BulkTransferRequest",
                    e => { e.ConfigureConsumer<BulkTransferConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.CreateIndividualKycUser",
                    e => { e.ConfigureConsumer<CreateIndividualKycUserConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.SetTransactionReceiptNumber",
                    e => { e.ConfigureConsumer<SetTransactionReceiptNumberConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.SaveReceiptRequest",
                    e => { e.ConfigureConsumer<SaveReceiptConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.SyncWalletBalanceDaily",
                    e => { e.ConfigureConsumer<SyncWalletBalanceDailyConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.CashbackTransferRequest",
                   e => { e.ConfigureConsumer<CashbackTransferConsumer>(context); });

                cfg.ReceiveEndpoint("Emoney.RemoveExpiredBlockages", e =>
                {
                    e.ConfigureConsumer<RemoveExpiredBlockagesConsumer>(context);
                });
                cfg.ReceiveEndpoint("Emoney.TodebDeclarations", e =>
                {
                    e.ConfigureConsumer<TodebDeclarationsConsumer>(context);
                });
            });
        });
    }
}
