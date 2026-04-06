using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.PricingProfiles;

public class PricingProfileItemDto : IMapFrom<PricingProfileItem>
{
    public Guid Id { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public WalletType WalletType { get; set; }
}
