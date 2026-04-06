namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Kart Silme", "tr")]
[LocalizedDisplay("Delete Card", "en")]
public class DeleteCardError : NotificationBase, INotificationEvent
{
    public EventNotificationField EventNotificationField { get; set; }
}