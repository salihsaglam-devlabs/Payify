using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class NotificationOrderDto
{
    public Guid Id { get; set; }
    public NotificationMethod NotificationMethod { get; set; }
    public DateTime PlannedSendDate { get; set; }
    public DateTime CompletionDate { get; set; }
    public NotificationStatus NotificationStatus { get; set; }
    public string TemplateName { get; set; }
    public int RecipientsCount { get; set; }
    public int AllHistoryCount { get; set; }
    public int SuccessHistoryCount { get; set; }
}