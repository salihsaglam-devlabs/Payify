using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class LinkTransaction : AuditEntity
{
    public Guid MerchantTransactionId { get; set; }
    public Guid CustomerId { get; set; }
    public string LinkCode { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public ChannelPaymentStatus LinkPaymentStatus { get; set; }
    public LinkType LinkType { get; set; }
    public string OrderId { get; set; }
    public string ThreeDSessionId { get; set; }
    public decimal Amount { get; set; }
    public int InstallmentCount { get; set; }
    public int Currency { get; set; }
    public decimal CommissionAmount { get; set; }
    public bool CommissionFromCustomer { get; set; }
    public bool Is3dRequired { get; set; }

}