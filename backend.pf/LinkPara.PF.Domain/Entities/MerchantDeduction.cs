using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantDeduction : AuditEntity, ITrackChange
{
    public Guid MerchantTransactionId { get;set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public int Currency { get; set; }
    public decimal TotalDeductionAmount { get; set; }
    public decimal RemainingDeductionAmount { get; set; }
    public DateTime ExecutionDate { get; set; }
    public DeductionType DeductionType { get; set; }
    public DeductionStatus DeductionStatus { get; set; }
    public Guid MerchantDueId { get;set; }
    public Guid PostingBalanceId { get;set; }
    public string ConversationId { get; set; }
    public decimal DeductionAmountWithCommission { get; set; }
    public Guid SubMerchantDeductionId { get; set; }
    public Guid? ProcessingId { get; set; }
    public DateTime ProcessingStartedAt { get; set; }
}