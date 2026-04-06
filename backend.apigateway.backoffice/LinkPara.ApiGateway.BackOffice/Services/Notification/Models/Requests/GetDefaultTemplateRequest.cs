using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class GetDefaultTemplateRequest : SearchQueryParams
{
    public string EventName { get; set; }
    public NotificationField NotificationField { get; set; }
}