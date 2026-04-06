using LinkPara.ApiGateway.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Services.Identity.Models.Responses
{
    public class UserInboxDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public MessageType MessageType { get; set; }
        public string ActionCode { get; set; }
        public bool IsRead { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Data { get; set; }
        public NotificationCategory Category { get; set; }
    }
}
