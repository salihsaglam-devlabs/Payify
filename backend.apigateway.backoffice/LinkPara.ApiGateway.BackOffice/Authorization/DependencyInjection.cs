using System.Net;
using System.Text;
using LinkPara.ApiGateway.BackOffice.Commons.Models.IdentityConfiguration;
using LinkPara.HttpProviders.Vault;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.ApiGateway.BackOffice.Authorization;

public static class DependencyInjection
{
    public static void AddJwtAuthorization(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
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
            ClockSkew = TimeSpan.FromSeconds(tokenSettings.ClockSkewDefaultSecond),
            TokenDecryptionKey = signingKey,
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
                OnChallenge = async (context) =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var response = new UnauthorizedAccessException();
                    await context.Response.WriteAsync(response.ToString());
                },
                OnAuthenticationFailed = async context =>
                {
                    Console.WriteLine($"Token validation failed: {context.Exception.Message}");
                    await Task.CompletedTask;
                }
            };
        });
    }
}