
using LinkPara.CampaignManagement.Application.Commons.Models.Responses;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IAuthorizationService
{
    Task<string> GetActiveTokenAsync();
    Task<string> RefreshTokenAsync(LoginResponse loginResult);
}
