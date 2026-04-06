namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Gün Sonu Hatası", "tr")]
[LocalizedDisplay("Posting Failure", "en")]
public class PostingFailed : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("Ortam", "tr")]
    [LocalizedDisplay("Environment", "en")]
    public string Environment { get; set; }
    
    [LocalizedDisplay("Hata", "tr")]
    [LocalizedDisplay("Exception", "en")]
    public string Exception { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}