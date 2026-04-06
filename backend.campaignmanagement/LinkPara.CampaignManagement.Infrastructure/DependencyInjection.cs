using LinkPara.SystemUser;
using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Models.EventBusConfiguration;
using LinkPara.CampaignManagement.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LinkPara.CampaignManagement.Infrastructure.Services;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.HttpProviders.Vault;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Identity;
using LinkPara.SharedModels.Migration;
using LinkPara.Cache;
using LinkPara.CampaignManagement.Infrastructure.Services.HttpClients;
using LinkPara.CampaignManagement.Application.Commons.Interfaces.HttpClients;
using LinkPara.SharedModels.Persistence;
using LinkPara.CampaignManagement.Application.Commons.Interfaces.Authorization;
using LinkPara.CampaignManagement.Infrastructure.Services.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LinkPara.CampaignManagement.Application.Commons.Models.LoginConfiguration;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.SharedModels.UrlModel;
using LinkPara.Audit;

namespace LinkPara.CampaignManagement.Infrastructure;

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
        var applicationUserService = await services.ConfigureApplicationUser("CampaignManagement", vaultClient);

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
        services.AddScoped<DbContext, CampaignManagementDbContext>();
        services.AddScoped<IDomainEventService, DomainEventService>(); 
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
        services.AddScoped<IIWalletAgreementService, IWalletAgreementService>();
        services.AddScoped<IIWalletLocationService, IWalletLocationService>();
        services.AddScoped<IIWalletCardService, IWalletCardService>();
        services.AddScoped<IIWalletQrCodeService, IWalletQrCodeService>();
        services.AddScoped<IIWalletLoginService, IWalletLoginService>();
        services.AddScoped<IIWalletChargeService, IWalletChargeService>();
        services.AddScoped<IIWalletCashbackService, IWalletCashbackService>();
        services.AddScoped<IAccountingService, AccountingService>(); 
        services.AddScoped<IIWalletOtpService, IWalletOtpService>();
        services.AddScoped<ISmsSenderService, SmsSenderService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddHttpClient<IIWalletHttpClient, IWalletHttpClient>(client =>
        {
            client.BaseAddress = new Uri(vaultClient.GetSecretValue<string>("CampaignManagementSecrets", "IWalletSettings", "Url"));
        });

        services.AddTransient<IEmoneyService, EMoneyService>();

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

        services.AddHttpClient<IParameterService, ParameterService>(client =>
        {
            services.ConfigureCustomHttpClient(client, serviceUrls.BusinessParameter);
        });

        return services;
    }

    private static void ConfigureCustomHttpClient(this IServiceCollection services, HttpClient client, string uriString)
    {
        string token = services.BuildServiceProvider().GetService<IApplicationUserService>().Token;

        client.BaseAddress = new Uri(uriString);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("CampaignManagementSecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<CampaignManagementDbContext>(
                        options => options
                    .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(CampaignManagementDbContext).Assembly.FullName))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<CampaignManagementDbContext>(
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

    public static void AddJwtProviderAuthorization(this IServiceCollection services, IVaultClient vaultClient)
    {
        var tokenSettings = vaultClient.GetSecretValue<TokenSettings>("SharedSecrets", "JwtConfiguration", "TokenSettings");

        var signingKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(tokenSettings.TokenAuthenticationSettings.SecretKey));

        var tokenAuthenticationSettings = tokenSettings.TokenAuthenticationSettings;

        var tokenProviderOptions = new TokenProviderOptions()
        {
            Audience = tokenAuthenticationSettings.Audience,
            Issuer = tokenAuthenticationSettings.Issuer,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            IsOtpTokenPathEnabled = false,
            Expiration = TimeSpan.FromMinutes(tokenSettings.TokenExpiryDefaultMinute)
        };

        services.AddSingleton(tokenProviderOptions);
        services.AddTransient<IJwtHelper, JwtHelper>();
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
            });
        });
    }
}