using LinkPara.Cache;
using LinkPara.HttpProviders.Vault;
using System.Reflection;
using System.Web;

namespace LinkPara.HealthCheck.Utility;

public static class Extension
{
    public static string GetQueryString(this object obj)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);

        foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties()
                     .Where(p => !p.GetIndexParameters().Any()))
        {
            var val = propertyInfo.GetValue(obj, null);

            if (val == null)
                continue;

            query[propertyInfo.Name] = val.ToString();
        }

        if (!query.HasKeys())
            return "";

        return "?" + query;
    }
    public static IVaultClient AddVault(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        var vaultClient = services.ConfigureVaultHttpClient(configuration);
        services.AddHttpClient<IVaultClient, VaultClient>(_ => (VaultClient)vaultClient);
        return vaultClient;
    }
}
