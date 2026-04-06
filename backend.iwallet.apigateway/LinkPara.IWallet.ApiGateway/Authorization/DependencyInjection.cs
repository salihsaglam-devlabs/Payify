using LinkPara.HttpProviders.Vault;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

namespace LinkPara.IWallet.ApiGateway.Authorization;

public static class DependencyInjection
{
    public static void AddJwtAuthorization(this IServiceCollection services, IVaultClient vaultClient)
    {
        var tokenSettings = vaultClient.GetSecretValue<TokenSettings>("SharedSecrets", "JwtConfiguration", "TokenSettings");

        var signingKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(tokenSettings.TokenAuthenticationSettings.SecretKey));

        var tokenAuthenticationSettings = tokenSettings.TokenAuthenticationSettings;

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = tokenAuthenticationSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = tokenAuthenticationSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            TokenDecryptionKey = signingKey,
        };
        var tokenProviderOptions = new TokenProviderOptions()
        {
            Audience = tokenAuthenticationSettings.Audience,
            Issuer = tokenAuthenticationSettings.Issuer,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            IsOtpTokenPathEnabled = false,
            Expiration = TimeSpan.FromMinutes(tokenSettings.TokenExpiryDefaultMinute)
        };

        services.AddAuthentication(cfg =>
        {
            cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = tokenValidationParameters;

            options.Events = new JwtBearerEvents
            {
                OnChallenge = (context) =>
                {
                    context.HandleResponse();
                    var response = new UnauthorizedAccessException();
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.WriteAsync(response.ToString()).Wait();
                    return Task.CompletedTask;
                }
            };
        });
    }
}
