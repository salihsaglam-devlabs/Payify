namespace LinkPara.Billing.Application.Features.Commissions;

public class CommissionWithAmountDetailDto
{
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCommissionAmount { get; set; }
}
