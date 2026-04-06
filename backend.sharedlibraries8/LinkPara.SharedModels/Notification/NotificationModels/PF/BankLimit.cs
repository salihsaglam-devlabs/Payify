namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Banka Limit Sorgulama", "tr")]
[LocalizedDisplay("Bank Limit Inquiry", "en")]
public class BankLimit : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("Banka Adı", "tr")]
    [LocalizedDisplay("Bank Name", "en")]
    public string BankName { get; set; }
    
    [LocalizedDisplay("Banka Limit Türü", "tr")]
    [LocalizedDisplay("Bank Limit Type", "en")]
    public string BankLimitType { get; set; }
    
    [LocalizedDisplay("Toplam Banka Limiti", "tr")]
    [LocalizedDisplay("Total Bank Limit", "en")]
    public string TotalBankLimit { get; set; }
    
    [LocalizedDisplay("Kullanılmış Banka Limiti", "tr")]
    [LocalizedDisplay("User Bank Limit", "en")]
    public string UsedBankLimit { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}