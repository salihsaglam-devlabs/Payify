namespace LinkPara.ApiGateway.CorporateWallet.Commons.Models.IdentityConfiguration;

public class LockoutSettings
{
    public bool AllowedForNewUsers { get; set; }
    public int DefaultLockoutTimeSpanInMinutes { get; set; }
    public int MaxFailedAccessAttempts { get; set; }
}