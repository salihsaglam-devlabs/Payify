using LinkPara.Kkb.Application;
using LinkPara.Kkb.Infrastructure;
using LinkPara.Kkb.Infrastructure.Persistence;
using LinkPara.HttpProviders.Vault;
using MediatR;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;
using LinkPara.Kkb.Application.Commons.Interfaces;
using LinkPara.Kkb.Application.Commons.Models.KkbSettings;
using LinkPara.Kkb.Infrastructure.ExternalServices.Kkb;

namespace LinkPara.Kkb.IntegrationTests;

[SetUpFixture]
public class Testing
{
    private static IConfigurationRoot _configuration;
    public static IServiceScopeFactory _scopeFactory;

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();

        var services = new ServiceCollection();

        services.AddSingleton(Mock.Of<IWebHostEnvironment>(w =>
            w.EnvironmentName == "Development" &&
            w.ApplicationName == "LinkPara.Kkb.API"));

        services.AddLogging();
        services.AddEndpointsApiExplorer();

        var vaultClient = services.AddVault(_configuration);
        services.AddApplication(_configuration);
        services.AddInfrastructure(_configuration, vaultClient);
        
        var kkbApiAuthorizationSettings = vaultClient.GetSecretValue<KkbApiAuthorizationSettings>("KkbSecrets", "KkbApiAuthorization");
        services.AddSingleton(settings => kkbApiAuthorizationSettings);

        services.AddHttpClient<IKkbAuthorizationService, KkbAuthorizationService>(client =>
        {
            client.BaseAddress = new Uri(kkbApiAuthorizationSettings.ApiUrl);
        });

        services.AddHttpClient<IKkbValidationService, KkbValidationService>(client =>
        {
            client.BaseAddress = new Uri(kkbApiAuthorizationSettings.ApiUrl);
        });

        _scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task<TEntity> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<KkbDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<KkbDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }
}