namespace LinkPara.Identity.Application.Common.Models.IdentityConfiguration;

public class PasswordSettings
{
    public int RequiredLength { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireDigit { get; set; }
    public bool RequireNonAlphanumeric { get; set; }
    public int PasswordTokenExpiryDefaultDay { get; set; } = 5;

}