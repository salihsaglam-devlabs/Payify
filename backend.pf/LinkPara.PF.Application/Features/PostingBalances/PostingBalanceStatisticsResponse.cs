namespace LinkPara.PF.Application.Features.PostingBalances;

public class PostingBalanceStatisticsResponse : TotalStatisticModel
{
    public Dictionary<Guid,MerchantPostingBalanceStatistic> MerchantStatistics { get; set; }
}

public class MerchantPostingBalanceStatistic : TotalStatisticModel
{
    public string MerchantName { get; set; }
}

public class TotalStatisticModel
{
    public int TotalTransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPayingAmount { get; set; }
    public decimal TotalPfCommissionAmount { get; set; }
    public decimal TotalBankCommissionAmount { get; set; }
    public decimal TotalDueAmount { get; set; }
    public decimal TotalDueTransferAmount { get; set; }
    public decimal TotalExcessReturnAmount { get; set; }
    public decimal TotalExcessReturnTransferAmount { get; set; }
    public decimal TotalExcessReturnOnCommissionAmount { get; set; }
    public decimal TotalChargebackAmount { get; set; }
    public decimal TotalChargebackCommissionAmount { get; set; }
    public decimal TotalChargebackTransferAmount { get; set; }
    public decimal TotalSuspiciousAmount { get; set; }
    public decimal TotalSuspiciousCommissionAmount { get; set; }
    public decimal TotalSuspiciousTransferAmount { get; set; }
    public decimal TotalSubmerchantDeductionAmount { get; set; }
    public decimal TotalParentMerchantCommissionAmount { get; set; }
}