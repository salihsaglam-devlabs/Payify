using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class PostingPfProfitDetail : AuditEntity
{
    public int AcquireBankCode { get; set; }
    public string BankName { get; set; }
    public decimal BankPayingAmount { get; set; }
    public int Currency { get; set; }
    public Guid PostingPfProfitId { get; set; }
    public PostingPfProfit PostingPfProfit { get; set; }
}