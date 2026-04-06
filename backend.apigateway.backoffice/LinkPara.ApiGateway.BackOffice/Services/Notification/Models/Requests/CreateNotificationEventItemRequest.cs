using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class CreateNotificationEventItemRequest
{
    public NotificationField NotificationField { get; set; }
    public NotificationMethod NotificationMethod { get; set; }
    public Guid TemplateId { get; set; } 
    public List<Guid> RoleIds { get; set; }
    public NotificationContactPersonType ContactPersonType { get; set; }
    public List<Guid> MerchantIds { get; set; }
    public string RecipientFilterJson { get; set; }
}