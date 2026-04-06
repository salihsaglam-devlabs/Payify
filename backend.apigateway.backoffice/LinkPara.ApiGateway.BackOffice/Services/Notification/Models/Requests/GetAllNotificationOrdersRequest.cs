using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class GetAllNotificationOrdersRequest : SearchQueryParams
{
    public Guid? NotificationItemId { get; set; }
    public string TemplateName { get; set; }
    public DateTime? PlannedSendDate { get; set; }
}