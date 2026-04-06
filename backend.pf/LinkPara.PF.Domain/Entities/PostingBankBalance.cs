using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;
public class PostingBankBalance : AuditEntity
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public int AcquireBankCode { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime OldPaymentDate { get; set; }
    public DateTime TransactionDate { get; set; }
    public int Currency { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPointAmount { get; set; }
    public decimal TotalBankCommissionAmount { get; set; }
    public decimal TotalAmountWithoutBankCommission { get; set; }
    public decimal TotalPfCommissionAmount { get; set; }
    public decimal TotalPfNetCommissionAmount { get; set; }
    public decimal TotalParentMerchantCommissionAmount { get; set; }
    public decimal TotalAmountWithoutCommissions { get; set; }
    public decimal TotalPayingAmount { get; set; }
    public decimal TotalReturnAmount { get; set; }
    public int TransactionCount { get; set; }
    public BatchStatus BatchStatus { get; set; }
    public BlockageStatus BlockageStatus { get; set; }
    public PostingAccountingStatus AccountingStatus { get; set; }
    public Guid? PostingBalanceId { get; set; }
    public Guid ParentMerchantId { get; set; }
    public PostingBalance PostingBalance { get; set; }
}