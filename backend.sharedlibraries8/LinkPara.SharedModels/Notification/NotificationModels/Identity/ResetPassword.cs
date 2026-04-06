namespace LinkPara.SharedModels.Notification.NotificationModels.Identity;

[LocalizedDisplay("Şifre Sıfırlama Talebi", "tr")]
[LocalizedDisplay("Reset Password Request", "en")]
public class ResetPassword : NotificationBase, INotificationOrder
{
    [LocalizedDisplay("Kullanıcı Id", "tr")]
    [LocalizedDisplay("User Id", "en")]
    public Guid UserId { get; set; }
    
    [LocalizedDisplay("Link", "tr")]
    [LocalizedDisplay("Link", "en")]
    public string Link { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}