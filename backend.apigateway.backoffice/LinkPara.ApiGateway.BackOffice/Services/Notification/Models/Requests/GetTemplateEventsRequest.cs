using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class GetTemplateEventsRequest : SearchQueryParams
{
    public ContentLanguage Language { get; set; }
    public string EventName { get; set; }
}