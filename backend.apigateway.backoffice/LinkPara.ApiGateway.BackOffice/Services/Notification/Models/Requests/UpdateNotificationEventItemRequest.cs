using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class UpdateNotificationEventItemRequest
{
    public Guid Id { get; set; }
    public NotificationField NotificationField { get; set; }
    public NotificationMethod NotificationMethod { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public List<Guid> RoleIds { get; set; }
    public NotificationContactPersonType ContactPersonType { get; set; }
    public List<Guid> MerchantIds { get; set; }
}