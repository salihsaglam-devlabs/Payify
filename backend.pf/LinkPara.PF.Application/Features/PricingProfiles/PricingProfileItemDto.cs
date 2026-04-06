using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.PricingProfiles;

public class PricingProfileItemDto : IMapFrom<PricingProfileItem>
{
    public Guid Id { get; set; }
    public ProfileCardType ProfileCardType { get; set; }
    public int InstallmentNumber { get; set; }
    public int InstallmentNumberEnd { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal ParentMerchantCommissionRate { get; set; }
    public int BlockedDayNumber { get; set; }
    public bool IsActive { get; set; }
    public List<PricingProfileInstallmentDto> PricingProfileInstallments { get; set; } = new List<PricingProfileInstallmentDto>();
}
