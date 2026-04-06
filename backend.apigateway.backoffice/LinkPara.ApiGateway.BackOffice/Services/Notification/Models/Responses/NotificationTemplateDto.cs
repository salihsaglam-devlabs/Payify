using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public NotificationTemplateType Type { get; set; }
    public string Name { get; set; }
    public string Subject { get; set; }
    public string Header { get; set; }
    public string Body { get; set; }
    public string Channel { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
