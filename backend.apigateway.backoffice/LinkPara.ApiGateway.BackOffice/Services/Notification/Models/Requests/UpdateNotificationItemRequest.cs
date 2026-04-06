using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class UpdateNotificationItemRequest
{
    public Guid Id { get; set; }
    public NotificationField NotificationField { get; set; }
    public NotificationMethod NotificationMethod { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan FirstSendTime { get; set; }
    public int RecurrenceNumber { get; set; }
    public NotificationRecurrenceType RecurrenceType { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public List<Guid> RoleIds { get; set; }
    public NotificationRecipientType RecipientType { get; set; }
    public List<Guid> RecipientIds { get; set; }
    public string RecipientFilterJson { get; set; }
    public NotificationContactPersonType ContactPersonType { get; set; }
}