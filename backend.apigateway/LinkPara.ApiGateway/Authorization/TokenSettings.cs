namespace LinkPara.ApiGateway.Authorization;

public class TokenSettings
{
    public TokenAuthenticationSettings TokenAuthenticationSettings { get; set; }
    public int TokenExpiryDefaultMinute { get; set; }
    public int ClockSkewDefaultSecond { get; set; } = 0;
}