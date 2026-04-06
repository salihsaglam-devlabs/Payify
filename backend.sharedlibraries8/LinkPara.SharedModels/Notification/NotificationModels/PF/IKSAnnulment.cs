namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("IKS Fesih", "tr")]
[LocalizedDisplay("IKS Annulmentt", "en")]
public class IKSAnnulment : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("ÜİY Numarası", "tr")]
    [LocalizedDisplay("Merchant Number", "en")]
    public string MerchantNumber { get; set; }
    
    [LocalizedDisplay("ÜİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string MerchantName { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}