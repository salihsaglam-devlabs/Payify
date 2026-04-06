using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class HostedPaymentTransaction : AuditEntity
{
    public Guid MerchantTransactionId { get; set; }
    public string TrackingId { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public ChannelPaymentStatus HppPaymentStatus { get; set; }
    public string OrderId { get; set; }
    public string ThreeDSessionId { get; set; }
    public decimal Amount { get; set; }
    public int InstallmentCount { get; set; }
    public int Currency { get; set; }
    public bool Is3dRequired { get; set; }
}