namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Aktif Terminal Cihazda Bulunmuyor", "tr")]
[LocalizedDisplay("Active Terminal Missing on Device", "en")]
public class ActiveTerminalMissingOnDevice : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("Banka Adı", "tr")]
    [LocalizedDisplay("Bank Name", "en")]
    public string BankName { get; set; }
    
    [LocalizedDisplay("Banka Kodu", "tr")]
    [LocalizedDisplay("Bank Acquire ID", "en")]
    public string AcqId { get; set; }
    
    [LocalizedDisplay("Terminal ID", "tr")]
    [LocalizedDisplay("Terminal ID", "en")]
    public string Tid { get; set; }
    
    [LocalizedDisplay("Merchant ID", "tr")]
    [LocalizedDisplay("Merchant ID", "en")]
    public string Mid { get; set; }
    
    [LocalizedDisplay("Tarih", "tr")]
    [LocalizedDisplay("Date", "en")]
    public DateTime Date { get; set; }
    
    [LocalizedDisplay("Cihaz Seri Numarası", "tr")]
    [LocalizedDisplay("Device Serial Number", "en")]
    public string SerialNumber { get; set; }
    
    [LocalizedDisplay("UİY Numarası", "tr")]
    [LocalizedDisplay("Merchant Number", "en")]
    public string MerchantNumber { get; set; }
    
    [LocalizedDisplay("UİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string MerchantName { get; set; }
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}