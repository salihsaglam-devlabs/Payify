using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class TemplateContentDto
{
    public Guid Id { get; set; } 
    public string Subject { get; set; }
    public string Content { get; set; }
    public ContentLanguage ContentLanguage { get; set; }
    public NotificationMethod NotificationMethod { get; set; }
    public AdvancedNotificationType NotificationType { get; set; }
    public string TemplateName { get; set; }
    public List<TemplateAttachmentDto> Attachments { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string CreatedNameBy { get; set; }
    public Guid TemplateId { get; set; }
    public RecordStatus RecordStatus { get; set; }
}