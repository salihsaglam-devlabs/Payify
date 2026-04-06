namespace LinkPara.Emoney.Application.Features.Wallets.Queries;

public class WithdrawPreviewResponse
{
    public decimal Amount { get; set; }
    public string ReceiverIBAN { get; set; }
    public string ReceiverName { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvRate { get; set; }
    public decimal BsmvTotal { get; set; }
    public decimal PricingAmount => Fee + CommissionAmount + BsmvTotal;
    public decimal TotalAmount => PricingAmount + Amount;
    public string PaymentType { get; set; }
}
