using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.Logging;

namespace LinkPara.HttpProviders.DbProvider;

public class DatabaseProviderService : IDatabaseProviderService
{
    private const string DefaultProvider = "Postgresql";

    private readonly ILogger<DatabaseProviderService> _logger;
    private readonly IVaultClient _vaultClient;

    public DatabaseProviderService(ILogger<DatabaseProviderService> logger,
        IVaultClient vaultClient)
    {
        _logger = logger;
        _vaultClient = vaultClient;
    }

    public async Task<string> GetProviderAsync()
    {
        try
        {
            var databaseProvider = await _vaultClient.GetSecretValueAsync<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

            if (string.IsNullOrEmpty(databaseProvider))
            {
                return await Task.FromResult(DefaultProvider);
            }

            return await Task.FromResult(databaseProvider);
        }
        catch (Exception exception)
        {
            _logger.LogError("Provider Fetch Error {Exception}", exception);
        }

        return DefaultProvider;
    }
}