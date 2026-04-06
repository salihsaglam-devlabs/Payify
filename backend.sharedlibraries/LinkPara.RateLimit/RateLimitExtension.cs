using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.RateLimiting;

namespace LinkPara.RateLimit;

public static class RateLimitExtension
{
    private static ILogger _logger;
    public static IApplicationBuilder UseMicroServiceRateLimiter(this IApplicationBuilder app, IConfiguration configuration, Func<OnRejectedContext, CancellationToken, ValueTask> onRejected = null)
    {
        return app;
    }

    public static void InitializeLogger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("RateLimitExtension");

    }

    public static IApplicationBuilder UseGatewayRateLimiter(this IApplicationBuilder app, IConfiguration configuration, Func<OnRejectedContext, CancellationToken, ValueTask> onRejected = null)
    {
        return app;
    }

    public static IApplicationBuilder UseGatewayRateLimiter(this IApplicationBuilder app, RateLimiterSettings rateLimitingSettings, string gatewayName, ILoggerFactory loggerFactory, Func<OnRejectedContext, CancellationToken, ValueTask> onRejected = null)
    {

        ArgumentNullException.ThrowIfNull(app);

        InitializeLogger(loggerFactory);

        if (!rateLimitingSettings.RateLimitingEnabled)
        {
            return app;
        }

        var gatewaySettings = rateLimitingSettings.GatewayRateLimiterSettings
        .Where(x => x.GatewayName == gatewayName)
        .FirstOrDefault();

        if (!gatewaySettings.GatewayRateLimitingEnabled)
        {
            return app;
        }

        RateLimiterOptions rateLimiterOptions = new RateLimiterOptions
        {
            GlobalLimiter = RateLimitConfigurator.GetGatewayPartitionedRateLimiter(gatewaySettings),
            RejectionStatusCode = (int)HttpStatusCode.TooManyRequests,
            OnRejected = (c, t) =>
            {
                if (rateLimitingSettings.LoggingEnabled)
                {
                    _logger.LogError(string.Concat(gatewayName, " - GatewayRateLimiter - (429)TooManyRequests", "\n",
                                                   "Ip : ", c.HttpContext.Connection?.RemoteIpAddress?.ToString(), "\n",
                                                   "Username : ", c.HttpContext.User.Identity?.Name ?? " - ", "\n",
                                                   "RequestUrl : ", c.HttpContext.Request.GetDisplayUrl(), "\n",
                                                   "RequestBody : ", new StreamReader(c.HttpContext.Request.Body).ReadToEnd()));
                }
                return new();
            }
        };

        if (gatewaySettings.NotificationRateLimitingEnabled)
        {
            rateLimiterOptions.AddPolicy("PreventNotificationBombAttack", httpContext =>
                   RateLimitPartition.GetFixedWindowLimiter(
                       partitionKey: gatewaySettings.UseIpInfoForNotificationRateLimitingEnabled ? httpContext.Connection?.RemoteIpAddress?.ToString() : string.Empty,
                       factory: partition => new FixedWindowRateLimiterOptions
                       {
                           PermitLimit = gatewaySettings.NotificationRateLimitingPermitLimit,
                           Window = TimeSpan.FromSeconds(gatewaySettings.NotificationRateLimitingWindowDuration)
                       }));
        }
        return app.UseRateLimiter(rateLimiterOptions);
    }
}