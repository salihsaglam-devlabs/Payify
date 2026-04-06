using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using LinkPara.Template.Application.Commons.Interfaces;
using LinkPara.Template.Application.Commons.Models.EventBusConfiguration;
using LinkPara.Template.Infrastructure.Caching;
using LinkPara.Template.Infrastructure.Persistence;
using LinkPara.Template.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Template.Infrastructure;

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
    IConfiguration configuration)
    {
        ConfigureServices(services);
        ConfigureDatabase(services, configuration);
        ConfigureMassTransit(services, configuration);

        return services;
    }
    private static void ConfigureServices(IServiceCollection services)
    {

        services.AddMemoryCache();

        services.AddScoped(typeof(IGenericRepository<>), typeof(Repository<>));
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IApplicationUserService, ApplicationUserService>();
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddDbContext<TemplateDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDb"));
        }
        else
        {
            services.AddDbContext<TemplateDbContext>(options =>
                options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly(typeof(TemplateDbContext).Assembly.FullName))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
        }
    }


    private static void ConfigureMassTransit(IServiceCollection services, IConfiguration configuration)
    {
        var eventBusSettings = new EventBusSettings();
        configuration.GetSection(nameof(eventBusSettings)).Bind(eventBusSettings);

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