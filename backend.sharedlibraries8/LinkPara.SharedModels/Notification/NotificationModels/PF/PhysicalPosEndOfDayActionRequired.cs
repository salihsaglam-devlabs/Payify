namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Fiziksel POS Gün Sonu Müdahale Gerekli", "tr")]
[LocalizedDisplay("Physical POS End Of Day Action Required", "en")]
public class PhysicalPosEndOfDayActionRequired : NotificationBase, INotificationEvent
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
    
    [LocalizedDisplay("Para Birimi", "tr")]
    [LocalizedDisplay("Currency", "en")]
    public string Currency { get; set; }
    
    [LocalizedDisplay("Kabul Edilemeyen Satış Adedi", "tr")]
    [LocalizedDisplay("Unacceptable Sale Count", "en")]
    public int UnacceptableSaleCount { get; set; }
    
    [LocalizedDisplay("Kabul Edilemeyen İptal Adedi", "tr")]
    [LocalizedDisplay("Unacceptable Void Count", "en")]
    public int UnacceptableVoidCount { get; set; }
    
    [LocalizedDisplay("Kabul Edilemeyen İade Adedi", "tr")]
    [LocalizedDisplay("Unacceptable Refund Count", "en")]
    public int UnacceptableRefundCount { get; set; }
    
    [LocalizedDisplay("Kabul Edilemeyen Taksitli Satış Adedi", "tr")]
    [LocalizedDisplay("Unacceptable Installment Sale Count", "en")]
    public int UnacceptableInstallmentSaleCount { get; set; }
    
    [LocalizedDisplay("Kabul Edilemeyen Satış Tutarı", "tr")]
    [LocalizedDisplay("Unacceptable Sale Amount", "en")]
    public decimal UnacceptableSaleAmount { get; set; }
    
    [LocalizedDisplay("Kabul Edilemeyen İptal Tutarı", "tr")]
    [LocalizedDisplay("Unacceptable Void Amount", "en")]
    public decimal UnacceptableVoidAmount { get; set; }
    
    [LocalizedDisplay("Kabul Edilemeyen İade Tutarı", "tr")]
    [LocalizedDisplay("Unacceptable Refund Amount", "en")]
    public decimal UnacceptableRefundAmount { get; set; }
    
    [LocalizedDisplay("Kabul Edilemeyen Taksitli Satış Tutarı", "tr")]
    [LocalizedDisplay("Unacceptable Installment Sale Amount", "en")]
    public decimal UnacceptableInstallmentSaleAmount { get; set; }
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.PF | EventNotificationField.Backoffice;
}