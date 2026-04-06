namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("UİY IBAN Değişikliği", "tr")]
[LocalizedDisplay("Merchant IBAN Changed", "en")]
public class MerchantIbanChanged : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("UİY Numarası", "tr")]
    [LocalizedDisplay("Merchant Number", "en")]
    public string MerchantNumber { get; set; }
    
    [LocalizedDisplay("UİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string MerchantName { get; set; }
    
    [LocalizedDisplay("Eski IBAN", "tr")]
    [LocalizedDisplay("Old IBAN", "en")]
    public string OldIban { get; set; }
    
    [LocalizedDisplay("Yeni IBAN", "tr")]
    [LocalizedDisplay("New IBAN", "en")]
    public string NewIban { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.PF | EventNotificationField.Backoffice;
}