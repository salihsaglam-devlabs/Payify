namespace LinkPara.IWallet.ApiGateway.Authorization;

public class TokenSettings
{
    public TokenAuthenticationSettings TokenAuthenticationSettings { get; set; }
    public int TokenExpiryDefaultMinute { get; set; }
}