using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class PostingBalanceDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public TransactionMerchantResponse Merchant { get; set; }
    public TimeoutTransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime? OldPaymentDate { get; set; }
    public int Currency { get; set; }
    public int TransactionCount { get; set; }
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
    public decimal TotalPerTransactionFee { get; set; }
    public decimal TotalChargebackAmount { get; set; }
    public decimal TotalChargebackCommissionAmount { get; set; }
    public decimal TotalChargebackTransferAmount { get; set; }
    public decimal TotalSuspiciousAmount { get; set; }
    public decimal TotalSuspiciousCommissionAmount { get; set; }
    public decimal TotalSuspiciousTransferAmount { get; set; }
    public decimal TotalExcessReturnAmount { get; set; }
    public decimal TotalExcessReturnTransferAmount { get; set; }
    public decimal TotalExcessReturnCommissionAmount { get; set; }
    public decimal TotalSubmerchantDeductionAmount { get; set; }
    public decimal TotalNegativeBalanceAmount { get; set; }
    public DateTime MoneyTransferPaymentDate { get; set; }
    public int RetryCount { get; set; }
    public BlockageStatus BlockageStatus { get; set; }
    public DateTime CreateDate { get; set; }
    public int MoneyTransferBankCode { get; set; }
    public string MoneyTransferBankName { get; set; }
    public PostingMoneyTransferStatus MoneyTransferStatus { get; set; }
    public PostingPaymentChannel PostingPaymentChannel { get; set; }
    public PostingBalanceType PostingBalanceType { get; set; }
}
