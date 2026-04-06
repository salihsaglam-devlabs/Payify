using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application;
using LinkPara.PF.Infrastructure;
using LinkPara.PF.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace LinkPara.PF.IntegrationTests;

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
            w.ApplicationName == "LinkPara.PF.API"));
        
        services.AddLogging();
        services.AddEndpointsApiExplorer();
        services.AddApplication();

        var vaultClient = services.ConfigureVaultHttpClient(_configuration);
        services.AddHttpClient<IVaultClient, VaultClient>(client => (VaultClient)vaultClient);

        services.AddInfrastructure(_configuration, vaultClient);
        
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

        var context = scope.ServiceProvider.GetRequiredService<PfDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<PfDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

}