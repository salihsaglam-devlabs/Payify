namespace LinkPara.ApiGateway.CorporateWallet.Commons.Models.IdentityConfiguration;

public class TokenAuthenticationSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string CookieName { get; set; }
}