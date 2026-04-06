using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.Installments;

public class InstallmentPricingResponse
{
    public string MerchantNumber { get; set; }
    public List<LoyaltyInstallmentPricing> LoyaltyInstallmentPricing { get; set; }
    public bool IsSucceed { get; set; }
    public string ConversationId { get; set; }
}