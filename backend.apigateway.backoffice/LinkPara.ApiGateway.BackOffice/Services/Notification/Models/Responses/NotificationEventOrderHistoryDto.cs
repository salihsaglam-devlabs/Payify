using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class NotificationEventOrderHistoryDto
{
    public Guid ReceiverId { get; set; }
    public Guid EventOrderId { get; set; }
    public NotificationMethod NotificationMethod { get; set; }
    public NotificationField NotificationField { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public string ReceiverEmail { get; set; }
    public string ReceiverPhone { get; set; }
    public string ReceiverPhoneCode { get; set; }
    public string ReceiverContext { get; set; }
    public string ReceiverFullName { get; set; }
    public DateTime SentDate { get; set; }
    public NotificationStatus Status { get; set; }
}