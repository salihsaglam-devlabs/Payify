using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.MerchantTransactions;

public class BankTransactionDto : IMapFrom<BankTransaction>
{
    public Guid Id { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public int Currency { get; set; }
    public int InstallmentCount { get; set; }
    public string CardNumber { get; set; }
    public bool IsReverse { get; set; }
    public DateTime ReverseDate { get; set; }
    public bool Is3ds { get; set; }
    public int IssuerBankCode { get; set; }
    public Bank IssuerBank { get; set; }
    public int AcquireBankCode { get; set; }
    public Bank AcquireBank { get; set; }
    public string MerchantCode { get; set; }
    public string SubMerchantCode { get; set; }
    public string BankOrderId { get; set; }
    public string RrnNumber { get; set; }
    public string ApprovalCode { get; set; }
    public string BankResponseCode { get; set; }
    public string BankResponseDescription { get; set; }
    public DateTime BankTransactionDate { get; set; }
    public DateTime TransactionStartDate { get; set; }
    public DateTime TransactionEndDate { get; set; }
    public Guid VposId { get; set; }
    public Guid MerchantTransactionId { get; set; }
}
