namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

public class TransferPreviewResponse
{
    public string SenderWalletNumber { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvRate { get; set; }
    public decimal BsmvTotal { get; set; }
    public decimal PricingAmount => Fee + CommissionAmount;
    public decimal TotalAmount => PricingAmount + Amount + BsmvTotal;
    public string ReceiverName { get; set; }
}
