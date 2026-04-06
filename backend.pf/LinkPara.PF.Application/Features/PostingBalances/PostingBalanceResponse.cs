using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Features.PostingBalances;

public class PostingBalanceResponse
{
    public int TotalTransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPayingAmount { get; set; }
    public decimal TotalPfCommissionAmount { get; set; }
    public decimal TotalBankCommissionAmount { get; set; }
    public decimal TotalParentMerchantCommissionAmount { get; set; }
    public decimal TotalDueAmount { get; set; }
    public decimal TotalDueTransferAmount { get; set; }
    public decimal TotalExcessReturnAmount { get; set; }
    public decimal TotalExcessReturnTransferAmount { get; set; }
    public decimal TotalExcessReturnOnCommissionAmount { get; set; }
    public decimal TotalSubmerchantDeductionAmount { get; set; }
    public decimal TotalChargebackAmount { get; set; }
    public decimal TotalChargebackCommissionAmount { get; set; }
    public decimal TotalChargebackTransferAmount { get; set; }
    public decimal TotalSuspiciousAmount { get; set; }
    public decimal TotalSuspiciousCommissionAmount { get; set; }
    public decimal TotalSuspiciousTransferAmount { get; set; }
    public PaginatedList<PostingBalanceDto> PostingBalances { get; set; }
}
