using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.RateLimiting;

namespace LinkPara.RateLimit;

public static class RateLimit
{
    private const int DefaultIdPermitLimit = 1000;
    private const int DefaultUserPermitLimit = 1000;
    private const int DefaultPermitLimit = 2000;
    private const int DefaultSeconds = 60;

    public static PartitionedRateLimiter<HttpContext> GetGatewayPartitionedRateLimiter(IConfiguration configuration)
    {
        var rateLimitSetting = new RateLimitSettings();

        configuration.GetSection("RateLimit").Bind(rateLimitSetting);

        var ipRateLimiter = GetIpRateLimiter(rateLimitSetting);
        var userRateLimiter = GetUserRateLimiter(rateLimitSetting);
        var requestLimit = GetRequestRateLimiter(rateLimitSetting);

        return PartitionedRateLimiter.CreateChained(ipRateLimiter, userRateLimiter, requestLimit);
    }

    public static PartitionedRateLimiter<HttpContext> GetMicroServicePartitionedRateLimiter(
        IConfiguration configuration)
    {
        var rateLimitSetting = new RateLimitSettings();

        configuration.GetSection("RateLimit").Bind(rateLimitSetting);

        var userRateLimiter = GetUserRateLimiter(rateLimitSetting);
        var requestLimiter = GetRequestRateLimiter(rateLimitSetting);

        return PartitionedRateLimiter.CreateChained(userRateLimiter, requestLimiter);
    }

    private static PartitionedRateLimiter<HttpContext> GetRequestRateLimiter(RateLimitSettings rateLimitSetting)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(string.Empty, options => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = rateLimitSetting.RequestLimitInMin <= 0
                    ? DefaultPermitLimit
                    : rateLimitSetting.RequestLimitInMin,
                Window = TimeSpan.FromSeconds(DefaultSeconds)
            }));
    }

    private static PartitionedRateLimiter<HttpContext> GetUserRateLimiter(RateLimitSettings rateLimitSetting)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(httpContext.User.Identity?.Name ?? httpContext.Request.Host.Value,
                options => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = rateLimitSetting.RequestLimitPerUserInMin <= 0
                        ? DefaultUserPermitLimit
                        : rateLimitSetting.RequestLimitPerUserInMin,
                    Window = TimeSpan.FromSeconds(DefaultSeconds)
                }));
    }

    private static PartitionedRateLimiter<HttpContext> GetIpRateLimiter(RateLimitSettings rateLimitSetting)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection?.RemoteIpAddress?.ToString() ?? httpContext.Request.Host.Value, options =>
                    new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = rateLimitSetting.RequestLimitPerIpInMin <= 0
                            ? DefaultIdPermitLimit
                            : rateLimitSetting.RequestLimitPerIpInMin,
                        Window = TimeSpan.FromSeconds(DefaultSeconds)
                    }));
    }
}