using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses
{
    public class UserLoginLastActivityDto
    {
        public DateTime? LastSucceededLogin { get; set; }
        public DateTime? LastLockedLogin { get; set; }
        public DateTime? LastFailedLogin { get; set; }
        public LoginResult LoginResult { get; set; }
    }
}
