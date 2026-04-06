namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

public class MerchantIbanChanged : NotificationBase, INotificationEvent
{
    public string MerchantNumber { get; set; }
    public string MerchantName { get; set; }
    public string OldIban { get; set; }
    public string NewIban { get; set; }
}