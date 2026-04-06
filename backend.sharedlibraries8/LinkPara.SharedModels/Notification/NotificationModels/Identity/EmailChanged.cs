namespace LinkPara.SharedModels.Notification.NotificationModels.Identity;

[LocalizedDisplay("Kullanıcı Emaili Değişikliği", "tr")]
[LocalizedDisplay("User Email Changed", "en")]
public class EmailChanged : NotificationBase, INotificationOrder
{
    [LocalizedDisplay("Kullanıcı Id", "tr")]
    [LocalizedDisplay("User Id", "en")]
    public Guid UserId { get; set; }
    
    [LocalizedDisplay("Yeni Email", "tr")]
    [LocalizedDisplay("New Email", "en")]
    public string NewValue { get; set; }
    
    [LocalizedDisplay("Değiştirilme Tarihi", "tr")]
    [LocalizedDisplay("Change Date", "en")]
    public string CurrentDate { get; set; }

    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}