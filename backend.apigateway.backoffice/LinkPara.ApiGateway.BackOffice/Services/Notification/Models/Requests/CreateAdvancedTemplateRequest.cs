using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class CreateAdvancedTemplateRequest
{
    public string Name { get; set; }
    public NotificationField NotificationField { get; set; }
    public NotificationMethod NotificationMethods { get; set; }
    public AdvancedNotificationType NotificationType { get; set; }
    public string EventName { get; set; }
    public List<TemplateContentDto> TemplateContents { get; set; }
    public bool SetAsDefault { get; set; }
    public bool IysCheckEnabled { get; set; }
}