using LinkPara.ApiGateway.Merchant.Commons.EventBusConfiguration;
using LinkPara.ApiGateway.Merchant.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Document.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Location.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Notification.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.InternalServices;
using LinkPara.Cache;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.UrlModel;
using MassTransit;

namespace LinkPara.ApiGateway.Merchant.Commons;

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

    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        services.ConfigureHttpClients(vaultClient);
        services.ConfigureMasstransit(configuration, vaultClient);
        services.ConfigureServices();

        return services;
    }

    public static IServiceCollection ConfigureMasstransit(this IServiceCollection services,
        IConfiguration configuration,
         IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");

        var eventBusSettings = new EventBusSettings();
        if (isLocalConfigurationEnabled) configuration.GetSection("LocalConfiguration:EventBusSettings").Bind(eventBusSettings);
        else eventBusSettings = vaultClient.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings", null);

        services.AddMassTransit(x =>
        {
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

        return services;
    }


    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IValidateUserService, ValidateUserService>();

        return services;
    }

    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");
        services.AddHttpClient<IUserHttpClient, UserHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<IOtpHttpClient, OtpHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });
        
        services.AddHttpClient<INotificationHttpClient, NotificationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
        });

        services.AddHttpClient<IAccountHttpClient, AccountHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<IAuthHttpClient, AuthHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<ICountryHttpClient, CountryHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Location);
        });

        services.AddHttpClient<IRoleHttpClient, RoleHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<IPricingProfileHttpClient, PricingProfileHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IAcquireBankHttpClient, AcquireBankHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IBankHttpClient, BankHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        
        services.AddHttpClient<IInstallmentHttpClient, InstallmentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantHttpClient, MerchantHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IParentMerchantHttpClient, ParentMerchantHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IParameterHttpClient, ParameterHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BusinessParameter);
        });

        services.AddHttpClient<ICurrencyHttpClient, CurrencyHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<IMerchantUserHttpClient, MerchantUserHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPostingBalanceHttpClient, PostingBalanceHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPostingHttpClient, PostingHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IMerchantTransactionHttpClient, MerchantTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPfTransactionHttpClient, PfTransactionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });

        services.AddHttpClient<IPfPaymentHttpClient, PfPaymentHttpClient>(client =>
        {            
            client.BaseAddress = new Uri(serviceUrls.PFApiGateway);
        });
        services.AddHttpClient<IScreenHttpClient, ScreenHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });
        services.AddHttpClient<ILinkHttpClient, LinkHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<IMerchantContentHttpClient, MerchantContentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<IMerchantNotificationHttpClient, MerchantNotificationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<IHostedPaymentHttpClient, HostedPaymentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<IAgreementDocumentHttpClient, AgreementDocumentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        services.AddHttpClient<ISecurityPictureHttpClient, SecurityPictureHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });
        services.AddHttpClient<ISubMerchantHttpClient, SubMerchantHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<ISubMerchantDocumentsHttpClient, SubMerchantDocumentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<ISubMerchantUserHttpClient, SubMerchantUserHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<ISubMerchantLimitsHttpClient, SubMerchantLimitsHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        services.AddHttpClient<IDocumentHttpClient, DocumentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Document);
        });
        services.AddHttpClient<IDocumentTypeHttpClient, DocumentTypeHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Document);
        });
        services.AddHttpClient<IMerchantPreApplicationHttpClient, MerchantPreApplicationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Pf);
        });
        return services;
    }
}