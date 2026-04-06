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
using LinkPara.SharedModels.Exceptions;
using Microsoft.Extensions.Logging;


namespace LinkPara.Authentication;

public static class JwtTokenAuthorization
{
    public static void AddJwtAuthorization(this IServiceCollection services, IVaultClient vaultClient,
        TokenSettings tokenSettings = null)
    {
        if (tokenSettings is null)
        {
            tokenSettings =
                vaultClient.GetSecretValue<TokenSettings>("SharedSecrets", "JwtConfiguration", "TokenSettings");
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
                OnChallenge = (context) =>
                {
                    var headers = context.HttpContext.Request.Headers;
                    var formattedHeaders = string.Join(Environment.NewLine, headers.Select(h => $"{h.Key}: {h.Value}"));

                    var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("JwtTokenAuthorization");
                    logger.LogError(
                        "JWT authentication challenge triggered. Path: {Path}, Method: {Method}, Headers: {Headers}",
                        context.HttpContext.Request.Path,
                        context.HttpContext.Request.Method,
                        formattedHeaders);

                    context.HandleResponse();
                    var response = new SharedModels.Exceptions.AuthorizationException(ErrorCode.Unauthorized,
                        "SHR Attempted to perform an unauthorized operation");
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var details = new ApiProblemDetail()
                    {
                        Detail = "AuthorizationException",
                        Code = ErrorCode.Unauthorized,
                        Type = "AuthorizationException",
                        Status = "false"
                    };
                    context.Response.WriteAsJsonAsync<ApiProblemDetail>(details).Wait();
                    return Task.CompletedTask;
                }
            };
        });
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();
    }
}