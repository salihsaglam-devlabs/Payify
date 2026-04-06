using LinkPara.CampaignManagement.Application.Commons.Models.LoginConfiguration;

namespace LinkPara.CampaignManagement.Application.Commons.IWalletConfigurations;

public class IWalletSettings
{
    public string Url { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string WalletId { get; set; }
    public string Code { get; set; }
    public string BranchName { get; set; }
    public string QrCodeTimeoutInSeconds { get; set; }
    public LoginCredentialSettings LoginCredentials { get; set; }
}
