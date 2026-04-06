using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class NotificationItemDto
{
    public Guid Id { get; set; }
    public NotificationField NotificationField { get; set; }
    public NotificationMethod NotificationMethod { get; set; }
    public Guid TemplateId { get; set; } 
    public TemplateDto Template { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime FirstSendDate { get; set; }
    public int RecurrenceNumber { get; set; }
    public NotificationRecurrenceType RecurrenceType { get; set; }
    public DateTime NextSendDate { get; set; }
    public DateTime LastSendDate { get; set; }
    public NotificationRecipientDto NotificationRecipient { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DateTime CreateDate { get; set; }
}