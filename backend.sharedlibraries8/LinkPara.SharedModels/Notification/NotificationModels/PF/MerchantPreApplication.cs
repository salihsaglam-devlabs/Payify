namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("ÜİY Ön Başvurusu", "tr")]
[LocalizedDisplay("Merchant Pre Application", "en")]
public class MerchantPreApplication : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("ÜİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string FullName { get; set; }
    
    [LocalizedDisplay("Telefon Numarası", "tr")]
    [LocalizedDisplay("Phone Number", "en")]
    public string PhoneNumber { get; set; }
    
    [LocalizedDisplay("Email", "tr")]
    [LocalizedDisplay("Email", "en")]
    public string Email { get; set; }
    
    [LocalizedDisplay("Ürün Tipi", "tr")]
    [LocalizedDisplay("Product Type", "en")]
    public string ProductType { get; set; }
    
    [LocalizedDisplay("Aylık Ciro", "tr")]
    [LocalizedDisplay("Monthly Turnover", "en")]
    public string MonthlyTurnover { get; set; }
    
    [LocalizedDisplay("Website", "tr")]
    [LocalizedDisplay("Website", "en")]
    public string Website { get; set; }
    
    [LocalizedDisplay("Başvuru Tarihi", "tr")]
    [LocalizedDisplay("Application Date", "en")]
    public string CreateDate { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}