using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class HostedPaymentInstallment : AuditEntity
{
    public Guid HostedPaymentId { get; set; }
    public CardNetwork CardNetwork { get; set; }
    public int Installment { get; set; }
    public decimal? Amount { get; set; }
    public HostedPayment HostedPayment { get; set; }
}