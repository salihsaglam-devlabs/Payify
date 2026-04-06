namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Gün Sonu Özeti", "tr")]
[LocalizedDisplay("Posting Summary", "en")]
public class PostingSummary : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("Gün Sonu Tarihi", "tr")]
    [LocalizedDisplay("Posting Date", "en")]
    public string Date { get; set; }
    
    [LocalizedDisplay("Gün Sonu Başlangıç Saati", "tr")]
    [LocalizedDisplay("Posting Start Hour", "en")]
    public string StartHour { get; set; }
    
    [LocalizedDisplay("Gün Sonu Bitiş Saati", "tr")]
    [LocalizedDisplay("Posting Finish Hour", "en")]
    public string FinishHour { get; set; }
    
    [LocalizedDisplay("Gün Sonu Süresi", "tr")]
    [LocalizedDisplay("Posting Duration", "en")]
    public string Duration { get; set; }
    
    [LocalizedDisplay("Toplam İşlem Sayısı", "tr")]
    [LocalizedDisplay("Total Transaction Count", "en")]
    public string TotalCount { get; set; }
    
    [LocalizedDisplay("Toplam Başarısız Sayısı", "tr")]
    [LocalizedDisplay("Total Failed Count", "en")]
    public string TotalFailCount { get; set; }
    
    [LocalizedDisplay("Toplam İşlem Tutarı", "tr")]
    [LocalizedDisplay("Total Transaction Amount", "en")]
    public string TotalTransactionAmount { get; set; }
    
    [LocalizedDisplay("Toplam Komisyon Tutarı", "tr")]
    [LocalizedDisplay("Total Commission Amount", "en")]
    public string TotalCommissionAmount { get; set; }
    
    [LocalizedDisplay("Toplam Banka Komisyon Tutarı", "tr")]
    [LocalizedDisplay("Total Bank Commission Amount", "en")]
    public string TotalBankCommissionAmount { get; set; }
    
    [LocalizedDisplay("Toplam PF Komisyon Tutarı", "tr")]
    [LocalizedDisplay("Total PF Commission Amount", "en")]
    public string TotalPfNetCommissionAmount { get; set; }
    
    [LocalizedDisplay("Toplam Ödeme Tutarı", "tr")]
    [LocalizedDisplay("Total Paying Amount", "en")]
    public string TotalPayingAmount { get; set; }
    
    [LocalizedDisplay("Toplam Gelecek Ödeme Tutarı", "tr")]
    [LocalizedDisplay("Total Future Paying Amount", "en")]
    public string TotalFuturePayingAmount { get; set; }
    
    [LocalizedDisplay("Toplam Aidat Tutarı", "tr")]
    [LocalizedDisplay("Total Due Amount", "en")]
    public string TotalDueAmount { get; set; }
    
    [LocalizedDisplay("Toplam Fazlalık İade Tutarı", "tr")]
    [LocalizedDisplay("Total Excess Return Amount", "en")]
    public string TotalExcessReturnAmount { get; set; }
    
    [LocalizedDisplay("Toplam Chargeback Tutarı", "tr")]
    [LocalizedDisplay("Total Chargeback Amount", "en")]
    public string TotalChargebackAmount { get; set; }
    
    [LocalizedDisplay("Toplam Şüpheli Tutar", "tr")]
    [LocalizedDisplay("Total Suspicious Amount", "en")]
    public string TotalSuspiciousAmount { get; set; }
    
    [LocalizedDisplay("Toplam Chargeback İptal Tutarı", "tr")]
    [LocalizedDisplay("Total Chargeback Cancel Amount", "en")]
    public string TotalChargebackCancelAmount { get; set; }
    
    [LocalizedDisplay("Toplam Şüpheli İptal Tutarı", "tr")]
    [LocalizedDisplay("Total Suspicious Cancel Amount", "en")]
    public string TotalSuspiciousCancelAmount { get; set; }
    
    [LocalizedDisplay("Toplam Tutar", "tr")]
    [LocalizedDisplay("Total Amount", "en")]
    public string TotalAmount { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}