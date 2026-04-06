namespace LinkPara.RateLimit;
public class RateLimitSettings
{
    public int RequestLimitPerUserInMin { get; set; }
    public int RequestLimitPerIpInMin { get; set; }
    public int RequestLimitInMin { get; set; }
    public bool IsEnable { get; set; }
}
