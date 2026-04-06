using LinkPara.Cache;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using LinkPara.PF.Pos.ApiGateway.Authentication;
using LinkPara.PF.Pos.ApiGateway.Authentication.SignaturePolicy;
using LinkPara.PF.Pos.ApiGateway.Commons;
using LinkPara.PF.Pos.ApiGateway.Models;

namespace LinkPara.PF.Pos.ApiGateway
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
            services.AddHttpContextAccessor();
            services.AddHttpClient<IUserService, UserService>(client =>
            {
                client.BaseAddress = new Uri(vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Identity"));
            });
            var applicationUserService = await services.ConfigureApplicationUser("PfPosGateway", vaultClient);

            services.AddSingleton<IApplicationUserService>(applicationUserService);
        }
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration, IVaultClient vaultClient)
        {
            ConfigureServices(services);
            ConfigureMassTransit(services, configuration, vaultClient);
            return services;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISignatureValidator, SignatureValidator>();
            services.AddScoped<IAuthorizationHandler, SignatureRequirementHandler>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<IHashGenerator, HashGenerator>();
            services.AddScoped<IPaymentApiLog, PfPaymentApiLog>();
        }


        private static void ConfigureMassTransit(IServiceCollection services, IConfiguration configuration,
            IVaultClient vaultClient)
        {
            var isLocalConfigurationEnabled = configuration.GetValue<bool>("LocalConfiguration:IsEnabled");
            var eventBusSettings = new EventBusSettings();
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

                });
            });
        }

    }
}
