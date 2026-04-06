using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class DefaultTemplateDto
{
    public string Name { get; set; }
    public NotificationField NotificationField { get; set; }
    public NotificationMethod NotificationMethods { get; set; }
    public string EventName { get; set; }
    
    public List<DefaultTemplateContentDto> DefaultTemplateContents { get; set; }
}