namespace LinkPara.SharedModels.Notification.NotificationModels.BTrans;

[LocalizedDisplay("BTrans Dosya Geri Alım Hatası", "tr")]
[LocalizedDisplay("Revert BTrans Document Failure", "en")]
public class BTransRevertFailed : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("Ortam", "tr")]
    [LocalizedDisplay("Environment", "en")]
    public string Environment { get; set; }
    
    [LocalizedDisplay("Hata", "tr")]
    [LocalizedDisplay("Exception", "en")]
    public string Exception { get; set; }

    [LocalizedDisplay("Doküman ID", "tr")]
    [LocalizedDisplay("Document ID", "en")]
    public Guid DocumentTrackId { get; set; }

    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}