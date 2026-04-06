namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("Risk Onay", "tr")]
[LocalizedDisplay("Risk Approval", "en")]
public class RiskApproval : NotificationBase, INotificationEvent
{
    [LocalizedDisplay("ÜİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string MerchantName { get; set; }
    
    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.Backoffice;
}