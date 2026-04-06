using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;

namespace LinkPara.RateLimit;

public static class RateLimitConfigurator
{
    public static PartitionedRateLimiter<HttpContext> GetGatewayPartitionedRateLimiter(RateLimiterGatewaySettings gatewaySettings)
    {
        var rateLimiterChainList = new List<PartitionedRateLimiter<HttpContext>>();
        if (gatewaySettings.RequestRateLimitingEnabled)
        {
            rateLimiterChainList.Add(GetRequestRateLimiter(gatewaySettings));
        }

        if (gatewaySettings.UserRateLimitingEnabled)
        {
            rateLimiterChainList.Add(GetUserRateLimiter(gatewaySettings));
        }

        if (gatewaySettings.IpRateLimitingEnabled)
        {
            rateLimiterChainList.Add(GetIpRateLimiter(gatewaySettings));
        }

        return PartitionedRateLimiter.CreateChained(rateLimiterChainList.ToArray());
    }

    private static PartitionedRateLimiter<HttpContext> GetRequestRateLimiter(RateLimiterGatewaySettings gatewaySettings)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(string.Empty, options => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = gatewaySettings.RequestRateLimitingPermitLimit,
                Window = TimeSpan.FromSeconds(gatewaySettings.RequestRateLimitingWindowDuration)
            }));
    }

    private static PartitionedRateLimiter<HttpContext> GetUserRateLimiter(RateLimiterGatewaySettings gatewaySettings)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(httpContext.User.Identity?.Name ?? string.Empty,
                options => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = gatewaySettings.UserRateLimitingPermitLimit,
                    Window = TimeSpan.FromSeconds(gatewaySettings.UserRateLimitingWindowDuration)
                }));
    }

    private static PartitionedRateLimiter<HttpContext> GetIpRateLimiter(RateLimiterGatewaySettings gatewaySettings)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection?.RemoteIpAddress?.ToString() ?? httpContext.Request.Host.Value, options =>
                    new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = gatewaySettings.IpRateLimitingPermitLimit,
                        Window = TimeSpan.FromSeconds(gatewaySettings.IpRateLimitingWindowDuration)
                    }));
    }
}