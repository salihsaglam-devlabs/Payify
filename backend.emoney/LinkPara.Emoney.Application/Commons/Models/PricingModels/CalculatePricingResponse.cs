namespace LinkPara.Emoney.Application.Commons.Models.PricingModels;

public class CalculatePricingResponse
{
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvRate { get; set; }
    public decimal BsmvTotal { get; set; }
    public decimal PricingAmount => Fee + CommissionAmount;
    public decimal TotalAmount => PricingAmount + Amount + BsmvTotal;
}