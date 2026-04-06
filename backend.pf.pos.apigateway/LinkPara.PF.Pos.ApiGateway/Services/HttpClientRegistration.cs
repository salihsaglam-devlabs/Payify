using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Pos.ApiGateway.Services.HttpClients;

namespace LinkPara.PF.Pos.ApiGateway.Services;

public static class HttpClientRegistration
{
    public static IServiceCollection RegisterHttpClients(this IServiceCollection services, IVaultClient vaultClient)
    {
        var pfServiceUrl = vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Pf");
        services.AddHttpClient<IMerchantDeviceHttpClient, MerchantDeviceHttpClient>(client =>
        {
            client.BaseAddress = new Uri(pfServiceUrl);
        });
        
        services.AddHttpClient<IPaxHttpClient, PaxHttpClient>(client =>
        {
            client.BaseAddress = new Uri(pfServiceUrl);
        });
        
        return services;
    }
}