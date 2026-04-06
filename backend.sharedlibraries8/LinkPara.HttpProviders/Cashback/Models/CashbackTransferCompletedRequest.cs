using LinkPara.HttpProviders.Cashback.Enums;

namespace LinkPara.HttpProviders.Cashback.Models;

public class CashbackTransferCompletedRequest
{
    public Guid EntitlementId { get; set; }
    public CashbackPaymentStatus CashbackPaymentStatus { get; set; }
    public DateTime PaymentDate { get; set; }
    public string FailedReason { get; set; }
    public CashbackNotificationDto NotificationInfo { get; set; }
}
