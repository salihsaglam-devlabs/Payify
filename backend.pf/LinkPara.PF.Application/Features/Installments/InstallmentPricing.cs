using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.Installments;

public class InstallmentPricing
{
    public ProfileCardType ProfileCardType { get; set; }
    public int InstallmentNumber { get; set; }
    public int InstallmentNumberEnd { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int BlockedDayNumber { get; set; }
    public bool IsActive { get; set; }
}