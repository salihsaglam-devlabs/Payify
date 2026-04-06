using LinkPara.ApiGateway.Card.Commons.Models;
using LinkPara.HttpProviders.Vault;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LinkPara.ApiGateway.Card.Authorization.BasicAuthorizations;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<BasicAuthMiddleware> _logger;

    public BasicAuthMiddleware(RequestDelegate next,
        IVaultClient vaultClient,
        ILogger<BasicAuthMiddleware> logger)
    {
        _next = next;
        _vaultClient = vaultClient;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var message = string.Empty;

        if (context.Request.Path.Value?.Contains("health", StringComparison.OrdinalIgnoreCase) == true)
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var auth))
        {
            message = $"Authorization Not Found : {context.Request.Headers.Authorization}";

            await RejectAsync(context, auth);

            return;
        }

        var encoded = auth.ToString().Split(" ").Last();
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        var parts = decoded.Split(':');

        var username = parts[0];
        var password = parts[1];

        var credentials = await _vaultClient.GetSecretValueAsync<ClientList>("CardGatewaySecrets", "Credentials");

        if (credentials.Clients.Count != 0)
        {
            var client = credentials.Clients.FirstOrDefault(s => s.Username == username && s.Password == password);

            if (client != null)
            {
                await _next(context);

                return;
            }
        }

        message = $"Authorization Error : {context.Request.Headers.Authorization}";

        await RejectAsync(context, message);

        return;
    }

    private async Task RejectAsync(HttpContext context, string message = null)
    {
        _logger.LogError(message);

        context.Response.StatusCode = 401;

        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Authorization Failed",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

        var result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };

        await context.Response.WriteAsJsonAsync(details);
    }
}


