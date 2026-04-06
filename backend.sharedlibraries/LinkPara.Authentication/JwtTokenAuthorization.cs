
using LinkPara.Authentication.Authorization.CustomAuthorizations;
using LinkPara.Authentication.Authorization.Scopes;
using LinkPara.Authentication.Models;
using LinkPara.HttpProviders.Vault;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;


namespace LinkPara.Authentication;

public static class JwtTokenAuthorization
{
   

    public static void AddJwtAuthorization(this IServiceCollection services, IVaultClient vaultClient,TokenSettings tokenSettings=null)
    {
        if(tokenSettings is null)
        {
             tokenSettings = vaultClient.GetSecretValue<TokenSettings>("SharedSecrets", "JwtConfiguration", "TokenSettings");
        }
      
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
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();
    }
}

