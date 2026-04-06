using LinkPara.SharedModels.Notification;

namespace LinkPara.SharedModels.BusModels.Commands.Notification;

public class NotificationDetailing
{
    public Guid OrderId { get; set; }
    public string MerchantNumber { get; set; }
    public string ContentLanguage { get; set; }
    public INotificationEvent NotificationEvent { get; set; }
}