namespace LinkPara.SharedModels.Notification.NotificationModels.Identity;

[LocalizedDisplay("Şifre Değiştirildi", "tr")]
[LocalizedDisplay("Password Changed", "en")]
public class PasswordChanged : NotificationBase, INotificationOrder
{
    [LocalizedDisplay("Kullanıcı Id", "tr")]
    [LocalizedDisplay("User Id", "en")]
    public Guid UserId { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}