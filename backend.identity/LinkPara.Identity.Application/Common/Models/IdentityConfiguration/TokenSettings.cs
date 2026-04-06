namespace LinkPara.Identity.Application.Common.Models.IdentityConfiguration;

public class TokenSettings
{
    public TokenAuthenticationSettings TokenAuthenticationSettings { get; set; }
    public int TokenExpiryDefaultMinute { get; set; }
    public int WebRefreshTokenExpiryDefaultMinute { get; set; }
    public int BackofficeRefreshTokenExpiryDefaultMinute { get; set; }
    public int MerchantRefreshTokenExpiryDefaultMinute { get; set; }
}