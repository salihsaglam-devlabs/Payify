
namespace LinkPara.CampaignManagement.Application.Features.IWalletLogins;

public class LoginResponseDto
{
    public string Token { get; set; }
    public string TokenType { get; set; }
    public DateTime ExpireDate { get; set; }
    public string Name { get; set; }
}
