using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class TemplateFilterQuery : SearchQueryParams
{
    public NotificationTemplateType? Type { get; set; }
    public string Name { get; set; }
    public string Header { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}
