using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class NotificationRecipientDto
{
    public Guid Id { get; set; }
    public Guid NotificationItemId { get; set; }
    public NotificationRecipientType NotificationRecipientType { get; set; }
    public List<Guid> RecipientIds { get; set; }
    public string RecipientFilterJson { get; set; }
    public NotificationContactPersonType ContactPersonType { get; set; }
    public List<Guid> RoleIds { get; set; }
    public int RecipientCount { get; set; }
}