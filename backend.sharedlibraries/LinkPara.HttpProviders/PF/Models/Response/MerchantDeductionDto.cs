using LinkPara.HttpProviders.PF.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.PF.Models.Response;

public class MerchantDeductionDto
{
    public Guid Id { get; set; }
    public Guid MerchantTransactionId { get; set; }
    public Guid MerchantId { get; set; }
    public MerchantDto Merchant { get; set; }
    public int Currency { get; set; }
    public decimal TotalDeductionAmount { get; set; }
    public decimal RemainingDeductionAmount { get; set; }
    public DateTime ExecutionDate { get; set; }
    public DeductionType DeductionType { get; set; }
    public DeductionStatus DeductionStatus { get; set; }
    public Guid MerchantDueId { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DateTime CreateDate { get; set; }
    public string ConversationId { get; set; }
    public decimal DeductionAmountWithCommission { get; set; }
    public Guid SubMerchantDeductionId { get; set; }
}