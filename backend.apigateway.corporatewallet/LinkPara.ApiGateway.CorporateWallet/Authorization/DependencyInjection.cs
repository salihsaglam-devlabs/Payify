using System.Net;
using System.Text;
using LinkPara.ApiGateway.CorporateWallet.Commons.Models.IdentityConfiguration;
using LinkPara.HttpProviders.Vault;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace LinkPara.ApiGateway.CorporateWallet.Authorization;


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
            ClockSkew = TimeSpan.Zero,
            TokenDecryptionKey = signingKey,
            RequireExpirationTime = true
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
                OnAuthenticationFailed = (context) =>
                {

                    context.Fail("Token expired");
                    context.Response.StatusCode = 401;

                    var details = new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Unauthorized",
                        Detail = "Token expired",
                        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                    };

                    var result = new ObjectResult(details)
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };

                    context.Response.WriteAsync(JsonConvert.SerializeObject(result));

                    return Task.CompletedTask;
                },
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