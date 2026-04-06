namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Fiziksel POS İşlemi Kabul Edilemedi", "tr")]
[LocalizedDisplay("Physical POS Transaction Unacceptable", "en")]
public class PhysicalPosTransactionUnacceptable : NotificationBase, INotificationEvent
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
    
    [LocalizedDisplay("Hata Kodu", "tr")]
    [LocalizedDisplay("Error Code", "en")]
    public string ErrorCode { get; set; }
    
    [LocalizedDisplay("Hata Mesajı", "tr")]
    [LocalizedDisplay("Error Message", "en")]
    public string ErrorMessage { get; set; }
    
    [LocalizedDisplay("İşlem Tipi", "tr")]
    [LocalizedDisplay("Transaction Type", "en")]
    public string Type { get; set; }
    
    [LocalizedDisplay("İşlem Durumu", "tr")]
    [LocalizedDisplay("Transaction Status", "en")]
    public string Status { get; set; }
    
    [LocalizedDisplay("UİY ID", "tr")]
    [LocalizedDisplay("Merchant ID", "en")]
    public Guid MerchantId { get; set; }
    
    [LocalizedDisplay("UİY Numarası", "tr")]
    [LocalizedDisplay("Merchant Number", "en")]
    public string MerchantNumber { get; set; }
    
    [LocalizedDisplay("UİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string MerchantName { get; set; }
    
    [LocalizedDisplay("Ödeme ID", "tr")]
    [LocalizedDisplay("Payment ID", "en")]
    public string PaymentId { get; set; }
    
    [LocalizedDisplay("Banka UİY ID", "tr")]
    [LocalizedDisplay("Bank Merchant ID", "en")]
    public string PosMerchantId { get; set; }
    
    [LocalizedDisplay("Banka Terminal ID", "tr")]
    [LocalizedDisplay("Bank Terminal ID", "en")]
    public string PosTerminalId { get; set; }
    
    [LocalizedDisplay("Tutar", "tr")]
    [LocalizedDisplay("Amount", "en")]
    public decimal Amount { get; set; }
    
    [LocalizedDisplay("Puan Tutarı", "tr")]
    [LocalizedDisplay("Point Amount", "en")]
    public decimal PointAmount { get; set; }
    
    [LocalizedDisplay("Kur", "tr")]
    [LocalizedDisplay("Currency", "en")]
    public string Currency { get; set; }
    
    [LocalizedDisplay("Taksit", "tr")]
    [LocalizedDisplay("Installment", "en")]
    public int Installment { get; set; }
    
    [LocalizedDisplay("Maskeli Kart Numarası", "tr")]
    [LocalizedDisplay("Masked Card Number", "en")]
    public string MaskedCardNo { get; set; }
    
    [LocalizedDisplay("Banka Referans ID", "tr")]
    [LocalizedDisplay("Bank Reference ID", "en")]
    public string BankRef { get; set; }
    
    [LocalizedDisplay("Bağlı İşlem Referans ID", "tr")]
    [LocalizedDisplay("Original Reference ID", "en")]
    public string OriginalRef { get; set; }
    
    [LocalizedDisplay("Banka RRN Numarası", "tr")]
    [LocalizedDisplay("Bank RRN Number", "en")]
    public string Rrn { get; set; }
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.PF | EventNotificationField.Backoffice;
}