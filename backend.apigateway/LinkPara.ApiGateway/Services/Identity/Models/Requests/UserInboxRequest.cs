using LinkPara.ApiGateway.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests
{
    public class UserInboxRequest
    {
        public Guid UserId { get; set; }
        public NotificationCategory? Category { get; set; }
    }
}
