using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class GetAllNotificationEventOrdersRequest : SearchQueryParams
{
    public Guid? NotificationEventItemId { get; set; }
    public string TemplateName { get; set; }
    public DateTime? SentDate { get; set; }
}