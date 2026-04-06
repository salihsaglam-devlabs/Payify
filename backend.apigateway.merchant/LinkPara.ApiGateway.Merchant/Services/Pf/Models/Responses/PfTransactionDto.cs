using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class PfTransactionDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public TimeoutTransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime OldPaymentDate { get; set; }
    public string CardNumber { get; set; }
    public string OrderId { get; set; }
    public int InstallmentCount { get; set; }
    public int InstallmentSequence { get; set; }
    public int Currency { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public decimal BankCommissionRate { get; set; }
    public decimal BankCommissionAmount { get; set; }
    public decimal AmountWithoutBankCommission { get; set; }
    public decimal PfCommissionRate { get; set; }
    public decimal PfPerTransactionFee { get; set; }
    public decimal ParentMerchantCommissionAmount { get; set; }
    public decimal ParentMerchantCommissionRate { get; set; }
    public decimal AmountWithoutParentMerchantCommission { get; set; }
    public decimal PfCommissionAmount { get; set; }
    public decimal PfNetCommissionAmount { get; set; }
    public decimal AmountWithoutCommissions { get; set; }
    public BlockageStatus BlockageStatus { get; set; }
    public Guid PostingBankBalanceId { get; set; }
    public Guid PostingBalanceId { get; set; }
    public Guid ParentMerchantId { get; set; }
    public DateTime TransactionStartDate { get; set; }
    public string ConversationId { get; set; } 
    public Guid MerchantDeductionId { get; set; }
    public Guid RelatedPostingBalanceId { get; set; }
    public Guid SubMerchantId { get; set; }
    public string SubMerchantName { get; set; }
    public string SubMerchantNumber { get; set; }
    public string EasySubMerchantName { get; set; }
    public string EasySubMerchantNumber { get; set; }
    public PfTransactionSource PfTransactionSource { get; set; }
    public Guid MerchantPhysicalPosId { get; set; }
    public bool IsPerInstallment { get; set; }
    public Guid MerchantInstallmentTransactionId { get; set; }
}
