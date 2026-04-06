using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class DeductionTransaction : AuditEntity
{
    public Guid MerchantId { get; set; }
    public Guid PostingBalanceId { get; set; }
    public Guid MerchantDeductionId { get; set; }
    public DeductionType DeductionType { get; set; }
    public decimal Amount { get; set; }
    public int Currency { get; set; }
}