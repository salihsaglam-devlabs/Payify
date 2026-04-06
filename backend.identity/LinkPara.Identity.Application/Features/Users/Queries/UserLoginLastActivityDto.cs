using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

namespace LinkPara.Identity.Application.Features.Users.Queries
{
    public class UserLoginLastActivityDto : IMapFrom<UserLoginLastActivity>
    {
        public DateTime? LastSucceededLogin { get; set; }
        public DateTime? LastLockedLogin { get; set; }
        public DateTime? LastFailedLogin { get; set; }
        public LoginResult LoginResult { get; set; }
        public int FailedLoginCount { get; set; }
    }
}
