using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.PricingModels;

public class PricingProfileItemModel
{
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public WalletType WalletType { get; set; }
}
