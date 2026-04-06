namespace LinkPara.SharedModels.Notification.NotificationModels.BTrans;

[LocalizedDisplay("BTrans Hatası", "tr")]
[LocalizedDisplay("BTrans Failed", "en")]
public class BTransFailed : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("Ortam", "tr")]
    [LocalizedDisplay("Environment", "en")]
    public string Environment { get; set; }
    
    [LocalizedDisplay("Hata Mesajı", "tr")]
    [LocalizedDisplay("Exception Message", "en")]
    public string Exception { get; set; }
    
    [LocalizedDisplay("Bildirim Kaynağı", "tr")]
    [LocalizedDisplay("Notification Source", "en")]
    public string Source { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}