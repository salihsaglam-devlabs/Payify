namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("´ÜİY Para Transferi Başarısız", "tr")]
[LocalizedDisplay("Merchant Money Transfer Failed", "en")]
public class PostingMoneyTransferFailed : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("UİY Numarası", "tr")]
    [LocalizedDisplay("Merchant Number", "en")]
    public string MerchantNumber { get; set; }
    
    [LocalizedDisplay("UİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string MerchantName { get; set; }
    
    [LocalizedDisplay("Ödeme Tarihi", "tr")]
    [LocalizedDisplay("Payment Date", "en")]
    public DateTime PaymentDate { get; set; }
    
    [LocalizedDisplay("IBAN", "tr")]
    [LocalizedDisplay("IBAN", "en")]
    public string Iban { get; set; }
    
    [LocalizedDisplay("Cüzdan Numarası", "tr")]
    [LocalizedDisplay("WalletNumber", "en")]
    public string WalletNumber { get; set; }
    
    [LocalizedDisplay("Para Transfer Banka Kodu", "tr")]
    [LocalizedDisplay("Money Transfer Bank Code", "en")]
    public int MoneyTransferBankCode { get; set; }
    
    [LocalizedDisplay("Para Transfer Banka Adı", "tr")]
    [LocalizedDisplay("Money Transfer Bank Name", "en")]
    public string MoneyTransferBankName { get; set; }
    
    [LocalizedDisplay("Toplam Ödeme Tutarı", "tr")]
    [LocalizedDisplay("Total Paying Amount", "en")]
    public decimal TotalPayingAmount { get; set; }
    
    [LocalizedDisplay("Para Transferi Referans ID", "tr")]
    [LocalizedDisplay("Money Transfer Reference Id", "en")]
    public Guid MoneyTransferReferenceId { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } =
        EventNotificationField.PF | EventNotificationField.Backoffice;
}