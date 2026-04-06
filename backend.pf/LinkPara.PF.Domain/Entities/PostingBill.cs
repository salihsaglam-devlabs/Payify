using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class PostingBill : AuditEntity
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDueAmount { get; set; }
    public decimal TotalPayingAmount { get; set; }
    public decimal TotalPfCommissionAmount { get; set; }
    public decimal TotalBankCommissionAmount { get; set; }
    public int Currency { get; set; }
    public Guid ClientReferenceId { get; set; }
    public int BillMonth { get; set; }
    public int BillYear { get; set; }
    public DateTime BillDate { get; set; }
    public string BillUrl { get; set; }
}