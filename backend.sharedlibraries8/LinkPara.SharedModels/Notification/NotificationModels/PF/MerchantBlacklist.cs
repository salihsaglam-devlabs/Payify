namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("ÜİY Kara Liste", "tr")]
[LocalizedDisplay("Merchant Blacklist", "en")]
public class MerchantBlacklist : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("UİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string MerchantName { get; set; }
    
    [LocalizedDisplay("Müşteri Adı", "tr")]
    [LocalizedDisplay("Customer Name", "en")]
    public string CustomerName { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}