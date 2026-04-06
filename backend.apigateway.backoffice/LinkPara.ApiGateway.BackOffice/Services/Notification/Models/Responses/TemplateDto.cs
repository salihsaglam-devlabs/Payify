using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public NotificationField NotificationField { get; set; }
    public NotificationMethod NotificationMethods { get; set; }
    public AdvancedNotificationType NotificationType { get; set; }
    public string EventName { get; set; }
    public List<TemplateContentDto> TemplateContents { get; set; }
    public bool IysCheckEnabled { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string CreatedNameBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
}