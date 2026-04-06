using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.Notification.Models;

public class TemplateContentRequest : SearchQueryParams
{
    public string TemplateName { get; set; }
    public NotificationType? NotificationType { get; set; }
}

public enum NotificationType
{
    Custom,
    Event,
    Order
}