using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class GetAllNotificationEventItemRequest : SearchQueryParams
{
    public NotificationField? NotificationField { get; set; }
    public Guid? TemplateId { get; set; }
    public NotificationMethod? NotificationMethod { get; set; }
    public Guid? RoleId { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}