using LinkPara.HttpProviders.Identity.Models.Enums;

namespace LinkPara.HttpProviders.Notification.Models;

public class NotificationRequest
{
    public string Topic { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public List<string> Tokens { get; set; }
}
