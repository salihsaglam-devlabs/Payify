using System.ComponentModel.DataAnnotations;

namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("UİY Ekstresi", "tr")]
[LocalizedDisplay("Merchant Statement", "en")]
public class MerchantStatement : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("Başlangıç Tarihi", "tr")]
    [LocalizedDisplay("Start Date", "en")]
    public string StartDate { get; set; }
    
    [Display(Name = "BitisTarihi")]
    [LocalizedDisplay("Bitiş Tarihi", "tr")]
    [LocalizedDisplay("End Date", "en")]
    public string EndDate { get; set; }
    
    [LocalizedDisplay("UİY Numarası", "tr")]
    [LocalizedDisplay("Merchant Number", "en")]
    public string MerchantNumber { get; set; }
    
    [LocalizedDisplay("UİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string MerchantName { get; set; }
    
    [LocalizedDisplay("Ekstre Ayı", "tr")]
    [LocalizedDisplay("Statement Month", "en")]
    public string StatementMonth { get; set; }
    
    [LocalizedDisplay("Ekstre Yılı", "tr")]
    [LocalizedDisplay("Statement Year", "en")]
    public string StatementYear { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.PF | EventNotificationField.Backoffice;
}