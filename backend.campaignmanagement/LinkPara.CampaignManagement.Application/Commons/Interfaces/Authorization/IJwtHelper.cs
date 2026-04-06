using LinkPara.CampaignManagement.Application.Features.IWalletLogins;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces.Authorization;

public interface IJwtHelper
{
    Task<LoginResponseDto> GenerateJwtTokenAsync(string id, TimeSpan? expireIn = null);
}
