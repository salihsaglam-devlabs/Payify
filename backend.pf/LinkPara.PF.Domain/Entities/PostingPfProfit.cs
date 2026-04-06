using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class PostingPfProfit : AuditEntity
{
    public DateTime PaymentDate { get; set; }
    public decimal AmountWithoutBankCommission { get; set; }
    public decimal TotalPayingAmount { get; set; }
    public decimal TotalPfNetCommissionAmount { get; set; }
    public decimal ProtectionTransferAmount { get; set; }
    public int Currency { get; set; }
    public List<PostingPfProfitDetail> PostingPfProfitDetails { get; set; }
}