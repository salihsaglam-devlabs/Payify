using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class DefaultTemplateContentDto
{
    public string Subject { get; set; }
    public string Content { get; set; }
    public ContentLanguage ContentLanguage { get; set; }
    public NotificationMethod NotificationMethod { get; set; }
    public Guid DefaultTemplateId { get; set; }
}