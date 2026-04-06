using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class UpdateNotificationTemplateRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Subject { get; set; }
    public string Header { get; set; }
    public string Body { get; set; }
    public string Channel { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
