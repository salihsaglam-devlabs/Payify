using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class CreateNotificationTemplateRequest
{
    public string Name { get; set; }
    public NotificationTemplateType Type { get; set; }
    public string Subject { get; set; }
    public string Header { get; set; }
    public string Body { get; set; }
    public string Channel { get; set; }
}
