namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

public class MerchantStatement : NotificationBase, INotificationEvent
{
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string MerchantNumber { get; set; }
    public string MerchantName { get; set; }
}