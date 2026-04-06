namespace LinkPara.ApiGateway.OpenBanking.Commons.Models.IdentityConfiguration;

public class IdentitySettings
{
    public PasswordSettings Password { get; set; }
    public LockoutSettings Lockout { get; set; }
    public UserOptions UserOptions { get; set; }
    public TokenSettings TokenSettings { get; set; }
}