using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface IJwtHelper
{
    Task<string> GenerateJwtTokenAsync(User user, bool rememberMe = false, string sessionId = null, UserSession session = null, TimeSpan? expireIn = null);
    Task<UserSession> GenerateUserRefreshTokenAsync(User user);
}