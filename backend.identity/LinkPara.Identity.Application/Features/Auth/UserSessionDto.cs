using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.Auth;

public class UserSessionDto : IMapFrom<UserSession>
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
