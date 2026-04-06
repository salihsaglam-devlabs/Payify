namespace LinkPara.RateLimit;
public class RateLimiterGatewaySettings
{   
    public string GatewayName { get; set; }
    public bool GatewayRateLimitingEnabled { get; set; }

    public bool IpRateLimitingEnabled { get; set; }
    public int IpRateLimitingPermitLimit { get; set; }
    public int IpRateLimitingWindowDuration { get; set; }

    public bool NotificationRateLimitingEnabled { get; set; }
    public int NotificationRateLimitingPermitLimit { get; set; }
    public int NotificationRateLimitingWindowDuration { get; set; }
    public bool UseIpInfoForNotificationRateLimitingEnabled { get; set; }

    public bool UserRateLimitingEnabled { get; set; }
    public int UserRateLimitingPermitLimit { get; set; }
    public int UserRateLimitingWindowDuration { get; set; }

    public bool RequestRateLimitingEnabled { get; set; }
    public int RequestRateLimitingPermitLimit { get; set; }
    public int RequestRateLimitingWindowDuration { get; set; }
}