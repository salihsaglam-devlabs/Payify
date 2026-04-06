namespace LinkPara.SharedModels.Notification.NotificationModels.Identity;

[LocalizedDisplay("Hesap Kilitleme Bilgisi", "tr")]
[LocalizedDisplay("Lock Out Information", "en")]
public class LockOutInformation : NotificationBase, INotificationOrder
{
    [LocalizedDisplay("Kullanıcı Id", "tr")]
    [LocalizedDisplay("User Id", "en")]
    public Guid UserId { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}