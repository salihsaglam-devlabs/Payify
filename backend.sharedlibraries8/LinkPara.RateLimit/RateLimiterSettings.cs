namespace LinkPara.RateLimit;
public class RateLimiterSettings
{    
    public bool RateLimitingEnabled { get; set; }   
    public bool LoggingEnabled { get; set; } 
    public List<RateLimiterGatewaySettings> GatewayRateLimiterSettings { get; set; }
}