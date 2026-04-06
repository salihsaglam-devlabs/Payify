using System.Text;
using LinkPara.Approval;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.EventBusConfiguration;
using LinkPara.Identity.Application.Common.Models.IdentityConfiguration;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Infrastructure.Authorization;
using LinkPara.Identity.Infrastructure.Persistence;
using LinkPara.Identity.Infrastructure.Services;
using LinkPara.Identity.Infrastructure.Services.Approval;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Migration;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.Identity.Infrastructure.Consumers;
using System.Reflection;
using LinkPara.HttpProviders.MultiFactor;
using LinkPara.SharedModels.UrlModel;
using Microsoft.AspNetCore.DataProtection;
using LinkPara.Identity.Infrastructure.Consumers.CronJobs;
using Microsoft.EntityFrameworkCore.Diagnostics;
using LinkPara.SharedModels.Persistence;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Notification;
using LinkPara.SharedModels.Notification;

namespace LinkPara.Identity.Infrastructure;

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

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        ConfigureServices(services, vaultClient);
        ConfigureDatabase(services, configuration, vaultClient);
        ConfigureMassTransit(services, configuration, vaultClient);

        return services;
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

            x.AddBus(context => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri($"rabbitmq://{eventBusSettings.Host}/"),
                                   h =>
                                   {
                                       h.Username(eventBusSettings.Username);
                                       h.Password(eventBusSettings.Password);
                                   });

                cfg.ReceiveEndpoint("Identity.DeleteUser",
                    e => { e.ConfigureConsumer<DeleteUserConsumer>(context); });

                cfg.ReceiveEndpoint("Identity.DeleteUserSession",
                    e => { e.ConfigureConsumer<DeleteUserSessionConsumer>(context); });

                cfg.ReceiveEndpoint("Identity.UserAutoLock",
                     e => { e.ConfigureConsumer<UserAutoLockConsumer>(context); });
                
                cfg.Message<INotificationEvent>(m =>
                {
                    m.SetEntityName("Notification.SystemEvent");
                });

                cfg.Message<INotificationOrder>(m =>
                {
                    m.SetEntityName("Notification.SystemOrder");
                });
            }));
        });
    }

    private static void ConfigureServices(IServiceCollection services, IVaultClient vaultClient)
    {
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
        services.AddScoped<IUserEmailService, UserEmailService>();
        services.AddScoped<IUserLoginService, UserLoginService>();
        services.AddScoped<INotificationTemplateParametersService, NotificationTemplateParametersService>();
        services.AddScoped<IEmailSender, EmailSenderService>();
        services.AddScoped<IContextProvider, CurrentContextProvider>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddSingleton<IApplicationUserService, ApplicationUserService>();
        services.AddScoped<IAgreementDocumentService, AgreementDocumentService>();
        services.AddScoped<IMigrationConfigurator, MigrationConfigurator>();
        services.AddScoped<IDeviceInfoService, DeviceInfoService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IScreenService, ScreenService>();
        services.AddScoped<IPushNotificationSender, PushNotificationSenderService>();
        services.AddScoped<ISmsSender, SmsSenderService>();
        services.AddScoped<IDigitalKycService, DigitalKycService>();
        services.AddScoped<IAppUserTokenService, AppUserTokenService>();
        services.AddScoped<IDbCommandInterceptor, LongQueryLogger>();
    }
    public static async Task ConfigureHttpClients(this IServiceCollection services, IVaultClient vaultClient)
    {
        var serviceUrls = vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");

        services.AddHttpClient<IUserService, UserService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Identity);
        });

        var userService = services.BuildServiceProvider().GetService<IAppUserTokenService>();

        var token = await userService.GetAppUserJwtTokenAsync();

        services.AddHttpClient<ISearchService, SearchService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Fraud);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        });

        services.AddHttpClient<IParameterService, ParameterService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.BusinessParameter);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        });

        services.AddHttpClient<IMultiFactorService, MultiFactorService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.SoftOtp);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        });

        services.AddHttpClient<IAccountService, AccountService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Emoney);
        });

        services.AddHttpClient<INotificationService, NotificationService>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.Notification);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        });
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration,
        IVaultClient vaultClient)
    {
        var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
        var connectionString =
            isLocalConfigurationEnabled ?
            configuration.GetValue<string>("LocalConfiguration:DefaultConnection")
            : vaultClient.GetSecretValue<string>("IdentitySecrets", "ConnectionStrings", "DefaultConnection");

        var databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                services
                    .AddDbContext<ApplicationDbContext>(
                        options => options
                    .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
                break;
            default:
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<ApplicationDbContext>((sp, options) => options
                    .UseNpgsql(connectionString, b => b.EnableRetryOnFailure())
                    .UseSnakeCaseNamingConvention()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .AddInterceptors(sp.GetRequiredService<IDbCommandInterceptor>()));
                break;
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment?.ToLowerInvariant() is not "development")
        {
            var migrator = services.BuildServiceProvider().GetService<IMigrationConfigurator>();
            migrator.Migrate(connectionString, databaseProvider);
        }
    }

    public static void AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddPasswordValidator<PasswordValidatorService>()
            .AddDefaultTokenProviders();


        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/identity"));

        var passwordSettings = vaultClient.GetSecretValue<PasswordSettings>("IdentitySecrets", "PasswordSettings");
        var lockoutSettings = vaultClient.GetSecretValue<LockoutSettings>("IdentitySecrets", "LockoutSettings");
        var userOptions = vaultClient.GetSecretValue<UserOptionSettings>("IdentitySecrets", "UserOptions");

        services.Configure<IdentityOptions>(opt =>
        {
            opt.Password.RequiredLength = passwordSettings.RequiredLength;
            opt.Password.RequireDigit = passwordSettings.RequireDigit;
            opt.Password.RequireLowercase = passwordSettings.RequireLowercase;
            opt.Password.RequireUppercase = passwordSettings.RequireUppercase;
            opt.Password.RequireNonAlphanumeric = passwordSettings.RequireNonAlphanumeric;

            opt.Lockout.AllowedForNewUsers = lockoutSettings.AllowedForNewUsers;
            opt.Lockout.MaxFailedAccessAttempts = lockoutSettings.MaxFailedAccessAttempts;
            if (lockoutSettings.AutoUnlock)
            {
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutSettings.DefaultLockoutTimeSpanInMinutes);
            }
            else
            {
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(99999);
            }
            opt.User.RequireUniqueEmail = userOptions.RequireUniqueEmail;
        });

        services.Configure<DataProtectionTokenProviderOptions>(options =>
                 options.TokenLifespan = TimeSpan.FromDays(passwordSettings.PasswordTokenExpiryDefaultDay));
    }

    public static void AddJwtProviderAuthorization(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
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
            Expiration = TimeSpan.FromMinutes(tokenSettings.TokenExpiryDefaultMinute),
            WebRefreshTokenExpiration = TimeSpan.FromMinutes(tokenSettings.WebRefreshTokenExpiryDefaultMinute),
            BackofficeRefreshTokenExpiration = TimeSpan.FromMinutes(tokenSettings.BackofficeRefreshTokenExpiryDefaultMinute),
            MerchantRefreshTokenExpiration = TimeSpan.FromMinutes(tokenSettings.MerchantRefreshTokenExpiryDefaultMinute)
        };

        services.AddSingleton(tokenProviderOptions);
        services.AddTransient<IJwtHelper, JwtHelper>();
    }

    public static void AddApprovalScreenService(this IServiceCollection services)
    {
        services.AddScoped<IApprovalService, ApprovalService>();

        services.AddScoped<IApprovalScreenFactory, IdentityApprovalScreenFactory>();
        services.AddScoped<UserScreenService>()
          .AddScoped<IApprovalScreenService, UserScreenService>();
        services.AddScoped<RoleScreenService>()
           .AddScoped<IApprovalScreenService, RoleScreenService>();
        services.AddScoped<QuestionScreenService>()
            .AddScoped<IApprovalScreenService, QuestionScreenService>();
        services.AddScoped<AgreementDocumentScreenService>()
            .AddScoped<IApprovalScreenService, AgreementDocumentScreenService>();

    }
    
    public static void ConfigureSeedData(this IServiceCollection services)
    {
        services.AddHostedService<SeedDataHostedService>();
    }
}
