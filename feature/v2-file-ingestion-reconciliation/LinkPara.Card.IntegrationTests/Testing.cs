using LinkPara.Card.Application;
using LinkPara.Card.Infrastructure;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.HttpProviders.Vault;
using MediatR;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

namespace LinkPara.Card.IntegrationTests;

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
            .AddJsonFile("appsettings.json", optional:true, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();
        
        var services = new ServiceCollection();

        services.AddSingleton(Mock.Of<IWebHostEnvironment>(w =>
            w.EnvironmentName == "Development" &&
            w.ApplicationName == "LinkPara.Card.API"));
        
        services.AddLogging();
        services.AddEndpointsApiExplorer();
        services.AddApplication(_configuration);

        var vaultClient = services.AddVault(_configuration);
        services.AddHttpClient<IVaultClient, VaultClient>(client => (VaultClient)vaultClient);
        services.AddInfrastructure(_configuration, vaultClient);
        services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
        
        _scopeFactory = services.BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>();
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

        var context = scope.ServiceProvider.GetRequiredService<CardDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CardDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

}