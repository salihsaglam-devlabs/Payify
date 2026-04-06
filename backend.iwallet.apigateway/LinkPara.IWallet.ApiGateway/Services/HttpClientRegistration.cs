using LinkPara.HttpProviders.Vault;
using LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;
using LinkPara.SharedModels.UrlModel;

namespace LinkPara.IWallet.ApiGateway.Services;

public static class HttpClientRegistration
{
    public static IServiceCollection RegisterHttpClients(this IServiceCollection services,
       IVaultClient vaultClient)
    {
        var serviceUrl = vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "CampaignManagement");
        services.AddHttpClient<ILoginHttpClient, LoginHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrl);
        });
        services.AddHttpClient<IChargeHttpClient, ChargeHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrl);
        });
        services.AddHttpClient<ICashBackHttpClient, CashBackHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrl);
        });
        services.AddHttpClient<IReverseChargeHttpClient, ReverseChargeHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrl);
        });
        services.AddHttpClient<IOtpHttpClient, OtpHttpClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrl);
        });
        return services;
    }
}
