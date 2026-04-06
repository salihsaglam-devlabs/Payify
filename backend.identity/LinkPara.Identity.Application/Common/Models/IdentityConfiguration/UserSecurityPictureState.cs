namespace LinkPara.Identity.Application.Common.Models.IdentityConfiguration;

public class UserSecurityPictureState
{
    public bool BackofficeEnabled { get; set; } = false;
    public bool MerchantEnabled { get; set; } = false;
    public bool WebEnabled { get; set; } = false;
}