using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Infrastructure.Persistence.SeedData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinkPara.Identity.Infrastructure.Services;

public class SeedDataHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;

    public SeedDataHostedService(IServiceProvider serviceProvider, IConfiguration configuration, IVaultClient vaultClient)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _vaultClient = vaultClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var args = Environment.GetCommandLineArgs();
        var seed = args.Any(x => x.Equals("seed", StringComparison.OrdinalIgnoreCase));
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!seed || (env?.ToLowerInvariant() is not "development"))
            return;

        using var scope = _serviceProvider.CreateScope();
        await Seeder.EnsureSeedData(scope.ServiceProvider, _vaultClient);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}