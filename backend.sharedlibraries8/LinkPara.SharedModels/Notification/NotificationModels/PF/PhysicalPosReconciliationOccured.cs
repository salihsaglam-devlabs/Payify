namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Fiziksel POS Gün Sonu İşlemi Tamamlanamadı", "tr")]
[LocalizedDisplay("Physical POS End Of Day Not Completed", "en")]
public class PhysicalPosReconciliationOccured : NotificationBase, INotificationEvent
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
    
    [LocalizedDisplay("Banka ID", "tr")]
    [LocalizedDisplay("Bank ID", "en")]
    public string BankId { get; set; }
    
    [LocalizedDisplay("Satış Adedi", "tr")]
    [LocalizedDisplay("Sale Count", "en")]
    public int SaleCount { get; set; }
    
    [LocalizedDisplay("İptal Adedi", "tr")]
    [LocalizedDisplay("Void Count", "en")]
    public int VoidCount { get; set; }
    
    [LocalizedDisplay("İade Adedi", "tr")]
    [LocalizedDisplay("Refund Count", "en")]
    public int RefundCount { get; set; }
    
    [LocalizedDisplay("Taksitli Satış Adedi", "tr")]
    [LocalizedDisplay("Installment Sale Count", "en")]
    public int InstallmentSaleCount { get; set; }
    
    [LocalizedDisplay("Satış Tutarı", "tr")]
    [LocalizedDisplay("Sale Amount", "en")]
    public decimal SaleAmount { get; set; }
    
    [LocalizedDisplay("İptal Tutarı", "tr")]
    [LocalizedDisplay("Void Amount", "en")]
    public decimal VoidAmount { get; set; }
    
    [LocalizedDisplay("İade Tutarı", "tr")]
    [LocalizedDisplay("Refund Amount", "en")]
    public decimal RefundAmount { get; set; }
    
    [LocalizedDisplay("Taksitli Satış Tutarı", "tr")]
    [LocalizedDisplay("Installment Sale Amount", "en")]
    public decimal InstallmentSaleAmount { get; set; }
    
    [LocalizedDisplay("Kur", "tr")]
    [LocalizedDisplay("Currency", "en")]
    public string Currency { get; set; }
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.PF | EventNotificationField.Backoffice;
}