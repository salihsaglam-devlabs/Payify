using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class UpdateAdvancedTemplateRequest
{
    public Guid TemplateId { get; set; }
    public string Name { get; set; }
    public string EventName { get; set; }
    public List<TemplateContentDto> TemplateContents { get; set; }
    public bool SetAsDefault { get; set; }
    public bool IysCheckEnabled { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public AdvancedNotificationType NotificationType { get; set; }
    public NotificationMethod NotificationMethods { get; set; }
}