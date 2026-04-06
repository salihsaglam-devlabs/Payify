using LinkPara.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.HttpProviders.Vault;

public static class VaultHttpClientConfiguration
{
    /// <summary>
    /// Initializes the VaultClient.
    /// <br></br>
    /// Tries to read configurations from EnvironmentVariables first. If not exist, then will try AppSettings.
    /// <br></br>
    /// <br></br>
    /// Parameter Names
    /// <br></br>
    /// EnvironmentVariables: VaultAddress, VaultRoleId, VaultSecretId.
    /// <br></br>
    /// AppSettings: Vault:Address, Vault:RoleId, Vault:SecretId.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IVaultClient ConfigureVaultHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        var vaultAddress = Environment.GetEnvironmentVariable("VaultAddress") ?? configuration["VaultSettings:Address"];
        var vaultRoleId = Environment.GetEnvironmentVariable("VaultRoleId") ?? configuration["VaultSettings:RoleId"];
        var vaultSecretId = Environment.GetEnvironmentVariable("VaultSecretId") ?? configuration["VaultSettings:SecretId"];

        var cacheService = services.BuildServiceProvider().GetService<ICacheService>();
        
        VaultClient vaultClient = new(new HttpClient
        {
            BaseAddress = new Uri(vaultAddress)
        }, vaultRoleId, vaultSecretId, cacheService);

        services.AddHttpClient<IVaultClient, VaultClient>(client => vaultClient);

        return vaultClient;
    }
}
