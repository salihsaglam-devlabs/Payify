using KuveytServiceReference;
using LinkPara.Approval;
using LinkPara.Audit;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Calendar;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.IKS;
using LinkPara.HttpProviders.KKB;
using LinkPara.HttpProviders.KPS;
using LinkPara.HttpProviders.Location;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.Notification;
using LinkPara.HttpProviders.Receipt;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Interfaces.Boa;
using LinkPara.PF.Application.Commons.Models.EventBusConfiguration;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Infrastructure.Boa;
using LinkPara.PF.Infrastructure.Consumers;
using LinkPara.PF.Infrastructure.Consumers.CronJobs;
using LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.NestPayInsuranceVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.IntegrationLogger;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.PF.Infrastructure.Posting;
using LinkPara.PF.Infrastructure.Services;
using LinkPara.PF.Infrastructure.Services.Approval;
using LinkPara.PF.Infrastructure.Services.PaymentServices;
using LinkPara.PF.Infrastructure.Services.Statements;
using LinkPara.PF.Infrastructure.Services.VposServices;
using LinkPara.Security;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Migration;
using LinkPara.SharedModels.Notification;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.UrlModel;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using System.Reflection;
using LinkPara.HttpProviders.BTrans;
using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos;
using LinkPara.PF.Infrastructure.Persistence.SeedData;
using LinkPara.PF.Infrastructure.PhysicalPos;
using LinkPara.PF.Infrastructure.PhysicalPos.Pax;
using LinkPara.PF.Infrastructure.PureSqlStore;
using AccountingProvider = LinkPara.HttpProviders.Accounting;
using FraudService = LinkPara.PF.Infrastructure.Services.FraudService;
using IFraudService = LinkPara.PF.Application.Commons.Interfaces.IFraudService;
using IOnUsPaymentService = LinkPara.PF.Application.Commons.Interfaces.IOnUsPaymentService;
using OnUsPaymentService = LinkPara.PF.Infrastructure.Services.PaymentServices.OnUsPaymentService;

namespace LinkPara.PF.Infrastructure;


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
        var applicationUserService = await services.ConfigureApplicationUser("PF", vaultClient);

        services.AddSingleton<IApplicationUserService>(applicationUserService);
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
    IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);        
        ConfigureHttpClients(services, vaultClient);

        QuestPDF.Settings.License = LicenseType.Community;
        
        return services;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<DbContext, PfDbContext>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<ICardBinService, CardBinService>();
        services.AddScoped<IAcquireBankService, AcquireBankService>();
        services.AddScoped<IPricingProfileService, PricingProfileService>();
        services.AddScoped<IMerchantPoolService, MerchantPoolService>();
        services.AddScoped<IVposService, VposService>();
        services.AddScoped<IMccService, MccService>();
        services.AddScoped<INaceService, NaceService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IHashGenerator, HashGenerator>();
        services.AddScoped<ISecureKeyGenerator, SecureKeyGenerator>();
        services.AddScoped<IMerchantService, MerchantService>();
        services.AddScoped<IMerchantDocumentService, MerchantDocumentService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IPosRouterService, PosRouterService>();
        services.AddScoped<ICardTokenService, CardTokenService>();
        services.AddScoped<IMerchantIntegratorService, MerchantIntegratorService>();
        services.AddScoped<IApiKeyGenerator, ApiKeyGenerator>();
        services.AddScoped<IResponseCodeService, ResponseCodeService>();
        services.AddScoped<IReverseService, ReverseService>();
        services.AddScoped<IReturnService, ReturnService>();
        services.AddScoped<IThreeDService, ThreeDService>();
        services.AddScoped<IInquireService, InquireService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IMerchantBlockageService, MerchantBlockageService>();
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<IMerchantHistoryService, MerchantHistoryService>();
        services.AddScoped<ICostProfileService, CostProfileService>();
        services.AddScoped<IMerchantUserService, MerchantUserService>();
        services.AddScoped<ILimitService, LimitService>();
        services.AddScoped<IMerchantBusinessPartnerService, MerchantBusinessPartnerService>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
        services.AddScoped<IMerchantPoolService, MerchantPoolService>();
        services.AddScoped<IMerchantReturnPoolService, MerchantReturnPoolService>();
        services.AddScoped<IMerchantStatementService, MerchantStatementService>();
        services.AddScoped<IEmailSender, EmailSenderService>();
        services.AddScoped<IPostingBalanceService, PostingBalanceService>();
        services.AddSingleton<IDataEncryptionKeyService, DataEncryptionKeyService>();
        services.AddSingleton<IRsaEncryptionService, RsaEncryptionService>();
        services.AddScoped<VakifVpos>().AddScoped<IVposApi, VakifVpos>();
        services.AddScoped<InterVpos>().AddScoped<IVposApi, InterVpos>();
        services.AddScoped<NestPayVpos>().AddScoped<IVposApi, NestPayVpos>();
        services.AddScoped<KuveytVpos>().AddScoped<IVposApi, KuveytVpos>();
        services.AddScoped<FinansVpos>().AddScoped<IVposApi, FinansVpos>();
        services.AddScoped<PosnetVpos>().AddScoped<IVposApi, PosnetVpos>();
        services.AddScoped<AkbankVpos>().AddScoped<IVposApi, AkbankVpos>();
        services.AddScoped<OzanPayVpos>().AddScoped<IVposApi, OzanPayVpos>();
        services.AddScoped<IsbankVpos>().AddScoped<IVposApi, IsbankVpos>();
        services.AddScoped<NestPayInsuranceVpos>().AddScoped<IVposApi, NestPayInsuranceVpos>();
        services.AddScoped<VakifInsuranceVpos>().AddScoped<IVposApi, VakifInsuranceVpos>();
        services.AddScoped<VposServiceFactory>();
        services.AddScoped<IMerchantTransactionService, MerchantTransactionService>();
        services.AddScoped<IMerchantLimitService, MerchantLimitService>();
        services.AddScoped<ILinkService, LinkService>();
        services.AddScoped<IInstallmentService, InstallmentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOrderNumberGeneratorService, OrderNumberGeneratorService>();
        services.AddScoped<ILinkPaymentService, LinkPaymentService>();
        services.AddScoped<IPointInquiryService, PointInquiryService>();
        services.AddScoped<IIksPfService, IksPfService>();
        services.AddScoped<IDbCommandInterceptor, LongQueryLogger>();
        services.AddScoped<IHostedPaymentService, HostedPaymentService>();
        services.AddScoped<ISecureRandomGenerator, SecureRandomGenerator>();
        services.AddScoped<IDueProfileService, DueProfileService>();
        services.AddScoped<IRestrictionService, RestrictionService>();
        services.AddScoped<IMerchantDeductionService, MerchantDeductionService>();
        services.AddScoped<IVirtualPosService, VirtualPosServiceClient>();
        services.AddScoped<IPaymentDetailService, PaymentDetailService>();
        services.AddScoped<IBankLimitService, BankLimitService>();
        services.AddScoped<IBankHealthCheckService, BankHealthCheckService>();
        services.AddScoped<IBankHealthCheckTransactionService, BankHealthCheckTransactionService>();
        services.AddScoped<ISubMerchantService, SubMerchantService>();
        services.AddScoped<SoapInspectorBehavior>();
        services.AddScoped<SoapMessageInspector>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IBasePaymentService, BasePaymentService>();
        services.AddScoped<IOnUsPaymentService, OnUsPaymentService>();
        services.AddScoped<IFraudService, FraudService>();
        services.AddScoped<ISubMerchantDocumentService, SubMerchantDocumentService>();
        services.AddScoped<ISubMerchantLimitService, SubMerchantLimitService>();
        services.AddScoped<IMerchantPreApplicationService, MerchantPreApplicationService>();
        services.AddScoped<ISubMerchantUserService, SubMerchantUserService>();
        services.AddScoped<INotificationTemplateParametersService, NotificationTemplateParametersService>();
        services.AddScoped<IBulkOperationsService, BulkOperationsService>();
        services.AddScoped<IPhysicalPosService, PhysicalPosService>();
        services.AddScoped<IDeviceInventoryService, DeviceInventoryService>();
        services.AddScoped<IPaxPosService, PaxPosService>();
        services.AddScoped<IMerchantPhysicalDeviceService, MerchantPhysicalDeviceService>();
        services.AddScoped<IDeviceInventoryHistoryService, DeviceInventoryHistoryService>();
        services.AddScoped<IUnacceptableTransactionService, UnacceptableTransactionService>();
        services.AddScoped<IEndOfDayService, EndOfDayService>();

        //Boa Services
        services.AddScoped<IBoaMerchantService, BoaMerchantService>();
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("PFSecrets", "ConnectionStrings", "DefaultConnection");

        string databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services.AddDbContext<PfDbContext>(
                    options => options.UseSqlServer(
                        connectionString,
                        b => b.MigrationsAssembly(typeof(PfDbContext).Assembly.FullName)
                    ).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                );
                services.AddScoped<IPureSqlStore, MsSqlStore>();
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<PfDbContext>((sp, options) => options
                    .UseNpgsql(connectionString, b => b.EnableRetryOnFailure())
                    .UseSnakeCaseNamingConvention()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .AddInterceptors(sp.GetRequiredService<IDbCommandInterceptor>()));
                services.AddScoped<IPureSqlStore, PostgreSqlStore>();
                break;
        }


        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment?.ToLowerInvariant() is not "development")
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

                cfg.ReceiveEndpoint("PF.DeleteCard", e =>
                {
                    e.ConfigureConsumer<DeleteCardConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.ProcessTimeout", e =>
                {
                    e.ConfigureConsumer<ProcessTimeoutConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.ProcessTimeoutItems", e =>
                {
                    e.ConfigureConsumer<ProcessTimeoutItemsConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.SignatureAuthentication", e =>
                {
                    e.ConfigureConsumer<SignatureAuthenticationLogConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.CostProfile", e =>
                {
                    e.ConfigureConsumer<CostProfileConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PricingProfile", e =>
                {
                    e.ConfigureConsumer<PricingProfileConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PaymentApiLog", e =>
                {
                    e.ConfigureConsumer<PaymentApiLogConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.CheckBlacklistControl", e =>
                {
                    e.ConfigureConsumer<CheckBlacklistControlConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.MerchantBlacklistControl", e =>
                {
                    e.ConfigureConsumer<MerchantBlacklistControlConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.SendMerchantStatement", e =>
                {
                    e.ConfigureConsumer<SendMerchantStatementConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.CreateMerchantStatement", e =>
                {
                    e.ConfigureConsumer<CreateMerchantStatementConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PostingTransferItems", e =>
                {
                    e.PrefetchCount = 1;
                    e.ConfigureConsumer<PostingTransferItemsConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("PF.PostingPfProfit",e =>
                {
                    e.ConfigureConsumer<PostingPfProfitConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PostingBatchManager", e =>
                {
                    e.ConfigureConsumer<PostingBatchManagerConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.SendPostingSummaryMail", e =>
                {
                    e.ConfigureConsumer<SendPostingSummaryMailConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PostingBilling", e =>
                {
                    e.ConfigureConsumer<PostingBillingConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PostingBillUrl", e =>
                {
                    e.ConfigureConsumer<PostingBillUrlConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.TransferCompleted", e =>
                {
                    e.ConfigureConsumer<MoneyTransferCompletedConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.TransferFailed", e =>
                {
                    e.ConfigureConsumer<MoneyTransferFailedConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.TransferReturned", e =>
                {
                    e.ConfigureConsumer<MoneyTransferReturnedConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.TransferSaved", e =>
                {
                    e.ConfigureConsumer<MoneyTransferSavedConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.IKSAnnulmentsQuery", e =>
                {
                    e.ConfigureConsumer<IKSAnnulmentsQueryConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.SendPosInformation", e =>
                {
                    e.ConfigureConsumer<SendPosInformationConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.DeletePaymentLink", e =>
                {
                    e.ConfigureConsumer<DeletePaymentLinkConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.HppWebhookNotification", e =>
                {
                    e.ConfigureConsumer<HppWebhookNotificationConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.TriggerHppWebhook", e =>
                {
                    e.ConfigureConsumer<TriggerHppWebhookConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.MerchantDueDeduction", e =>
                {
                    e.ConfigureConsumer<MerchantDueDeductionConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.UpdateCustomerNumber", e =>
                {
                    e.ConfigureConsumer<UpdateCustomerNumberConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("PF.PublishTransferItems", e =>
                {
                    e.ConfigureConsumer<PostingPublishTransferItemsConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("PF.PostingUpdateBalanceFields", e =>
                {
                    e.ConfigureConsumer<PostingUpdateBalanceFieldsConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("PF.PostingUpdateBalanceItems", e =>
                {
                    e.ConfigureConsumer<PostingUpdateBalanceItemsConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.IncrementLimits", e =>
                {
                    e.ConfigureConsumer<IncrementLimitsConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.DecrementLimits", e =>
                {
                    e.ConfigureConsumer<DecrementLimitsConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.UpdateHealthCheck", e =>
                {
                    e.ConfigureConsumer<UpdateHealthCheckConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.ReturnBTransReport", e =>
                {
                    e.PrefetchCount = 1;
                    e.ConfigureConsumer<ReturnBTransReportConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.AuthPostAuthBTransReport", e =>
                {
                    e.PrefetchCount = 1;
                    e.ConfigureConsumer<AuthPostAuthBTransReportConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PostingDailyControl", e =>
                {
                    e.ConfigureConsumer<PostingDailyControlConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.OnUsWebhook", e =>
                {
                    e.ConfigureConsumer<OnUsWebhookConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.DeleteBankHealthCheckTransaction", e =>
                {
                    e.ConfigureConsumer<DeleteBankHealthCheckTransactionConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.CheckBankLimitStatus", e =>
                {
                    e.ConfigureConsumer<CheckBankLimitStatusConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("PF.CheckMerchantVposStatus", e =>
                {
                    e.ConfigureConsumer<CheckMerchantVposStatusConsumer>(context);
                });
                
                cfg.Message<INotificationEvent>(m =>
                {
                    m.SetEntityName("Notification.SystemEvent");
                });

                cfg.Message<INotificationOrder>(m =>
                {
                    m.SetEntityName("Notification.SystemOrder");
                });

                cfg.ReceiveEndpoint("PF.IKSTerminalIdUpdated", e =>
                {
                    e.ConfigureConsumer<IKSTerminalUpdatedConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.UpdateTransactionPaymentDate", e =>
                {
                    e.ConfigureConsumer<UpdateTransactionPaymentDateConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PublishTransactionPaymentDate", e =>
                {
                    e.ConfigureConsumer<PublishTransactionPaymentDateConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.CreateIksMerchant", e =>
                {
                    e.ConfigureConsumer<CreateIksMerchantConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PhysicalPosReconciliation", e =>
                {
                    e.ConfigureConsumer<PhysicalPosReconciliationConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.CreateIksTerminal", e =>
                {
                    e.ConfigureConsumer<CreateIksTerminalConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PostingInitiateMoneyTransfer", e =>
                {
                    e.PrefetchCount = 1;
                    e.ConcurrentMessageLimit = 1;
                    e.UseMessageRetry(r => r.None()); 
                    e.ConfigureConsumer<PostingInitiateMoneyTransferConsumer>(context);
                });

                cfg.ReceiveEndpoint("PF.PostingMoneyTransferMonitor", e =>
                {
                    e.ConfigureConsumer<PostingMoneyTransferMonitorConsumer>(context);
                });
            });
        });
    }

    private static void ConfigureHttpClients(IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");

        services.AddHttpClient<ICalendarService, CalendarService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Calendar);
        });

        services.AddHttpClient<IParameterService, ParameterService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.BusinessParameter);
        });

        services.AddHttpClient<ISearchService, SearchService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Fraud);
        });
        services.AddHttpClient<IFraudTransactionService, FraudTransactionService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Fraud);
        });
        services.AddHttpClient<ICustomerService, CustomerService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.CustomerManagement);
        });
        services.AddHttpClient<IMoneyTransferService, MoneyTransferService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.MoneyTransfer);
        });
        services.AddHttpClient<ISourceBankAccountService, SourceBankAccountService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.MoneyTransfer);
        });
        services.AddHttpClient<IIKSService, IKSService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.IKS);
        });
        services.AddHttpClient<IKKBService, KKBService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.KKB);
        });
        services.AddHttpClient<IKpsService, KpsService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.KPS);
        });

        services.AddHttpClient<ILocationService, LocationService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Location);
        });

        services.AddHttpClient<AccountingProvider.IInvoiceService, AccountingProvider.InvoiceService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Accounting);
        });

        services.AddHttpClient<INotificationService, NotificationService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Notification);
        });

        services.AddHttpClient<IRoleService, RoleService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Identity);
        });
        
        services.AddHttpClient<HttpProviders.Emoney.IOnUsPaymentService, HttpProviders.Emoney.OnUsPaymentService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Emoney);
        });
        
        services.AddHttpClient<IProvisionService, ProvisionService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Emoney);
        });
        
        services.AddHttpClient<IReceiptService, ReceiptService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.Receipt);
        });
        
        services.AddHttpClient<IBTransPosInformationService, BTransPosInformationService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.BTrans);
        });

		services.AddHttpClient<IPfReturnTransactionService, PfReturnTransactionService>(client =>
		{
			services.ConfigureCustomHttpClient(client, serviceUrls.MoneyTransfer);
		});
	}
    private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString)
    {
        string token = services.BuildServiceProvider().GetService<IApplicationUserService>().Token;

        client.BaseAddress = new Uri(uriString);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    public static void AddApprovalScreenService(this IServiceCollection services)
    {
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IApprovalScreenFactory, PfApprovalScreenFactory>();
        services.AddScoped<AcquireBankScreenService>()
                .AddScoped<IApprovalScreenService, AcquireBankScreenService>();
        services.AddScoped<CardBinScreenService>()
                .AddScoped<IApprovalScreenService, CardBinScreenService>();
        services.AddScoped<MerchantCategoryCodeScreenService>()
                .AddScoped<IApprovalScreenService, MerchantCategoryCodeScreenService>();
        services.AddScoped<VPosScreenService>()
                .AddScoped<IApprovalScreenService, VPosScreenService>();
        services.AddScoped<PricingProfileScreenService>()
                .AddScoped<IApprovalScreenService, PricingProfileScreenService>();
        services.AddScoped<MerchantPoolScreenService>()
                .AddScoped<IApprovalScreenService, MerchantPoolScreenService>();
        services.AddScoped<MerchantScreenService>()
               .AddScoped<IApprovalScreenService, MerchantScreenService>();
        services.AddScoped<MerchantTransactionScreenService>()
               .AddScoped<IApprovalScreenService, MerchantTransactionScreenService>();
        services.AddScoped<DueProfileScreenService>()
               .AddScoped<IApprovalScreenService, DueProfileScreenService>();
        services.AddScoped<MerchantBlockagesScreenService>()
               .AddScoped<IApprovalScreenService, MerchantBlockagesScreenService>();
        services.AddScoped<MerchantBusinessPartnerScreenService>()
               .AddScoped<IApprovalScreenService, MerchantBusinessPartnerScreenService>();
        services.AddScoped<MerchantDueScreenService>()
               .AddScoped<IApprovalScreenService, MerchantDueScreenService>();
        services.AddScoped<MerchantIntegratorsScreenService>()
               .AddScoped<IApprovalScreenService, MerchantIntegratorsScreenService>();
        services.AddScoped<MerchantLimitScreenService>()
               .AddScoped<IApprovalScreenService, MerchantLimitScreenService>();
        services.AddScoped<MerchantReturnPoolScreenService>()
               .AddScoped<IApprovalScreenService, MerchantReturnPoolScreenService>();
        services.AddScoped<MerchantUserScreenService>()
               .AddScoped<IApprovalScreenService, MerchantUserScreenService>();
        services.AddScoped<PostingBalanceScreenService>()
               .AddScoped<IApprovalScreenService, PostingBalanceScreenService>();
        services.AddScoped<BankLimitScreenService>()
               .AddScoped<IApprovalScreenService, BankLimitScreenService>();
        services.AddScoped<BankHealthCheckScreenService>()
               .AddScoped<IApprovalScreenService, BankHealthCheckScreenService>();
        services.AddScoped<CostProfileScreenService>()
               .AddScoped<IApprovalScreenService, CostProfileScreenService>();
    }

    public static void AddPostingBatches(this IServiceCollection services)
    {
        services.AddScoped<IPostingBatchFactory, PostingBatchFactory>();
        services.AddScoped<IPostingBatch<PostingTransfer>, PostingTransferBatch>();
        services.AddScoped<IPostingBatch<PostingTransferValidation>, PostingTransferValidationBatch>();
        services.AddScoped<IPostingBatch<PostingMerchantBlockage>, PostingMerchantBlockageBatch>();
        services.AddScoped<IPostingBatch<PostingBankBalancer>, PostingBankBalancerBatch>();
        services.AddScoped<IPostingBatch<PostingGrandBalancer>, PostingGrandBalancerBatch>();
        services.AddScoped<IPostingBatch<PostingDeductionCalculator>, PostingDeductionCalculationBatch>();
        services.AddScoped<IPostingBatch<PostingDeductionBalancer>, PostingDeductionBalancerBatch>();
        services.AddScoped<IPostingBatch<PostingPosBlockage>, PostingPosBlockageAccountingBatch>();
        services.AddScoped<IPostingBatch<PostingParentMerchantBalancer>, PostingParentMerchantBalancerBatch>();
        services.AddScoped<IPostingBatch<PostingDeductionTransfer>, PostingDeductionTransferBatch>();
    }

    public static void ConfigureSeedData(this IServiceCollection services)
    {
        services.AddHostedService<SeedDataHostedService>();
    }
}