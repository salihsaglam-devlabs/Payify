using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class PricingProfileItemDto
{
    public Guid Id { get; set; }
    public ProfileCardType ProfileCardType { get; set; }
    public int InstallmentNumber { get; set; }
    public int InstallmentNumberEnd { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal ParentMerchantCommissionRate { get; set; }
    public int BlockedDayNumber { get; set; }
    public bool IsActive { get; set; }
    public List<PricingProfileInstallmentDto> PricingProfileInstallments { get; set; }
}

public class PricingProfileInstallmentDto
{
    public Guid Id { get; set; }
    public int InstallmentSequence { get; set; }
    public int BlockedDayNumber { get; set; }
}