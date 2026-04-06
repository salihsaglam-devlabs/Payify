using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class PostingBalance : AuditEntity
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime OldPaymentDate { get; set; }
    public int Currency { get; set; }
    public string Iban { get; set; }
    public string WalletNumber { get; set; }
    public int MoneyTransferBankCode { get; set; }
    public string MoneyTransferBankName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPointAmount { get; set; }
    public decimal TotalBankCommissionAmount { get; set; }
    public decimal TotalAmountWithoutBankCommission { get; set; }
    public decimal TotalPfCommissionAmount { get; set; }
    public decimal TotalPfNetCommissionAmount { get; set; }
    public decimal TotalParentMerchantCommissionAmount { get; set; }
    public decimal TotalAmountWithoutCommissions { get; set; }
    public decimal TotalDueAmount { get; set; }
    public decimal TotalDueTransferAmount { get; set; }
    public decimal TotalPayingAmount { get; set; }
    public decimal TotalChargebackAmount { get; set; }
    public decimal TotalChargebackCommissionAmount { get; set; }
    public decimal TotalChargebackTransferAmount { get; set; }
    public decimal TotalSuspiciousAmount { get; set; }
    public decimal TotalSuspiciousCommissionAmount { get; set; }
    public decimal TotalSuspiciousTransferAmount { get; set; }
    public decimal TotalExcessReturnAmount { get; set; }
    public decimal TotalExcessReturnTransferAmount { get; set; }
    public decimal TotalExcessReturnOnCommissionAmount { get; set; }
    public decimal TotalNegativeBalanceAmount { get; set; }
    public DateTime MoneyTransferPaymentDate { get; set; }
    public PostingMoneyTransferStatus MoneyTransferStatus { get; set; }
    public Guid MoneyTransferReferenceId { get; set; }
    public Guid TransactionSourceId { get; set; }
    public int RetryCount { get; set; }
    public int TransactionCount { get; set; }
    public BatchStatus BatchStatus { get; set; }
    public BlockageStatus BlockageStatus { get; set; }
    public PostingAccountingStatus AccountingStatus { get; set; }
    public PostingBalanceType PostingBalanceType { get; set; }
    public PostingBTransStatus BTransStatus { get; set; }
    public PostingPaymentChannel PostingPaymentChannel { get; set; }
    public List<PostingBankBalance> PostingBankBalances { get; set; }
    public Guid ParentMerchantId { get; set; }
    public Guid? ProcessingId { get; set; }
    public DateTime ProcessingStartedAt { get; set; }
}