using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class GetAllNotificationItemsRequest : SearchQueryParams
{
    public NotificationField? NotificationField { get; set; }
    public Guid? TemplateId { get; set; }
    public NotificationMethod? NotificationMethod { get; set; }
    public Guid? RoleId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public DateTime? FirstSendDate { get; set; }
    public NotificationRecurrenceType? RecurrenceType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public int? RecurrenceNumber { get; set; }
    public DateTime? NextSendDateStart { get; set; }
    public DateTime? NextSendDateEnd { get; set; }
}