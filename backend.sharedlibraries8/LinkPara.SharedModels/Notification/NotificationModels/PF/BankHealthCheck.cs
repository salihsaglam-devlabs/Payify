namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Banka Sağlık Durumu Sorgulama", "tr")]
[LocalizedDisplay("Bank Health Check Inquiry", "en")]
public class BankHealthCheck : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("Banka Adı", "tr")]
    [LocalizedDisplay("Bank Name", "en")]
    public string BankName { get; set; }
    
    [LocalizedDisplay("Hata Oranı", "tr")]
    [LocalizedDisplay("Fail Rate", "en")]
    public string FailRate { get; set; }
    
    [LocalizedDisplay("Yeni Durum", "tr")]
    [LocalizedDisplay("New Status", "en")]
    public string NewCheckType { get; set; }
    
    [LocalizedDisplay("Eski Durum", "tr")]
    [LocalizedDisplay("Old Status", "en")]
    public string OldCheckType { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}