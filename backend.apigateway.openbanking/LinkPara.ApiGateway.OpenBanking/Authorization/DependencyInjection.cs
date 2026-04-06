using LinkPara.ApiGateway.OpenBanking.Commons.OpenBankingConfiguration;
using LinkPara.HttpProviders.Vault;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.ApiGateway.OpenBanking.Authorization;

public static class DependencyInjection
{
    public static void AddJwtAuthorization(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var openBankingHhsSettings = vaultClient.GetSecretValue<OpenBankingTokenSettings>("SharedSecrets", "OpenBankingHhsSettings", null);
        var openBankingYosSettings = vaultClient.GetSecretValue<OpenBankingTokenSettings>("SharedSecrets", "OpenBankingYosSettings", null);

        services.AddAuthentication()
        .AddJwtBearer("HHS",options =>
        {
            options.Authority = openBankingHhsSettings.Authority;
            options.RequireHttpsMetadata = openBankingHhsSettings.RequireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = openBankingHhsSettings.ValidateAudience
            };
        })
        .AddJwtBearer("YOS", options =>
        {
            options.Authority = openBankingYosSettings.Authority;
            options.RequireHttpsMetadata = openBankingYosSettings.RequireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = openBankingYosSettings.ValidateAudience
            };
        });
    }
}