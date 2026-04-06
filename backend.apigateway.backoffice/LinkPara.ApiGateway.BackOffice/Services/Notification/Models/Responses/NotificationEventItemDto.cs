using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class NotificationEventItemDto
{
    public Guid Id { get; set; }
    public NotificationField NotificationField { get; set; }
    public NotificationMethod NotificationMethod { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public Guid TemplateId { get; set; } 
    public List<Guid> RoleIds { get; set; }
    public TemplateDto Template { get; set; }
    public NotificationContactPersonType ContactPersonType { get; set; }
    public List<Guid> MerchantIds { get; set; }
    public int RecipientCount { get; set; }
    public DateTime CreateDate { get; set; }
}