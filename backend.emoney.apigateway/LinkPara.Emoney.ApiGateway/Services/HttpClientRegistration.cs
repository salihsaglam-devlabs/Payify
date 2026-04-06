using LinkPara.Emoney.ApiGateway.Services.HttpClients;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.UrlModel;

namespace LinkPara.Emoney.ApiGateway.Services;

public static class HttpClientRegistration
{
    public static IServiceCollection RegisterHttpClients(this IServiceCollection services, IVaultClient vaultClient)
    {
        var emoneyServiceUrl = vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Emoney");
        services.AddHttpClient<IApiKeyHttpClient, ApiKeyHttpClient>(client =>
        {
            client.BaseAddress = new Uri(emoneyServiceUrl);
        });
        services.AddHttpClient<IProvisionHttpClient, ProvisionHttpClient>(client =>
        {
            client.BaseAddress = new Uri(emoneyServiceUrl);
        });
        return services;
    }
}
