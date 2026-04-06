namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("BTrans Gönderim Hatası", "tr")]
[LocalizedDisplay("Send BTrans Failure", "en")]
public class SendBTransFailed : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("Ortam", "tr")]
    [LocalizedDisplay("Environment", "en")]
    public string Environment { get; set; }
    
    [LocalizedDisplay("Hata", "tr")]
    [LocalizedDisplay("Exception", "en")]
    public string Exception { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}