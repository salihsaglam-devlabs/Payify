using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.Installments;

public class LoyaltyInstallmentPricing
{
    public CardNetwork CardNetwork { get; set; }
    public List<InstallmentPricing> InstallmentPricings { get; set; }
}