namespace LinkPara.SharedModels.Notification.NotificationModels.Identity;

[LocalizedDisplay("Bireysel Kullanıcı Katılımı", "tr")]
[LocalizedDisplay("Individual User Onboarding", "en")]
public class UserOnboarding : NotificationBase, INotificationOrder
{
    [LocalizedDisplay("Link", "tr")]
    [LocalizedDisplay("Link", "en")]
    public string Link { get; set; }
    
    [LocalizedDisplay("Kullanıcı Adı", "tr")]
    [LocalizedDisplay("User Name", "en")]
    public string Username { get; set; }
    
    [LocalizedDisplay("Kullanıcı ID", "tr")]
    [LocalizedDisplay("User ID", "en")]
    public Guid UserId { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}