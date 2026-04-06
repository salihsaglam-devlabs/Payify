namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Fiziksel POS Mutabakat Sağlanamadı", "tr")]
[LocalizedDisplay("Physical POS Reconciliation Failed", "en")]
public class PhysicalPosReconciliationFailed : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("İşlem Tarihi", "tr")]
    [LocalizedDisplay("Transaction Date", "en")]
    public DateTime Date { get; set; }
    
    [LocalizedDisplay("Sağlayıcı", "tr")]
    [LocalizedDisplay("Vendor", "en")]
    public string Vendor { get; set; }
    
    [LocalizedDisplay("Cihaz Seri Numarası", "tr")]
    [LocalizedDisplay("Device Serial Number", "en")]
    public string SerialNumber { get; set; }
    
    [LocalizedDisplay("UİY ID", "tr")]
    [LocalizedDisplay("Merchant ID", "en")]
    public Guid MerchantId { get; set; }
    
    [LocalizedDisplay("UİY Numarası", "tr")]
    [LocalizedDisplay("Merchant Number", "en")]
    public string MerchantNumber { get; set; }
    
    [LocalizedDisplay("UİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string MerchantName { get; set; }
    
    [LocalizedDisplay("Banka UİY ID", "tr")]
    [LocalizedDisplay("Bank Merchant ID", "en")]
    public string PosMerchantId { get; set; }
    
    [LocalizedDisplay("Banka Terminal ID", "tr")]
    [LocalizedDisplay("Bank Terminal ID", "en")]
    public string PosTerminalId { get; set; }
    
    [LocalizedDisplay("Batch ID", "tr")]
    [LocalizedDisplay("Batch ID", "en")]
    public string BatchId { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.PF | EventNotificationField.Backoffice;
}