
using LinkPara.CampaignManagement.Application.Features.IWalletLogins;
using LinkPara.CampaignManagement.Application.Features.IWalletLogins.Commands.Login;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IIWalletLoginService
{
    Task<LoginResponseDto> LoginAsync(LoginCommand request);
}
