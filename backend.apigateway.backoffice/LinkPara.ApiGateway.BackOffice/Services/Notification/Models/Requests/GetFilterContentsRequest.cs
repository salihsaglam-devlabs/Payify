using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class GetFilterContentsRequest : SearchQueryParams
{
    public string TemplateName { get; set; }
    public NotificationField? NotificationField { get; set; }
    public NotificationMethod? NotificationMethods { get; set; }
    public AdvancedNotificationType? NotificationType { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public string CreatedNameBy { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public DateTime? UpdateDateStart { get; set; }
    public DateTime? UpdateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}