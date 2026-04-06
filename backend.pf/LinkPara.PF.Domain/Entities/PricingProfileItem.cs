using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;
public class PricingProfileItem : AuditEntity, ITrackChange 
{
    public ProfileCardType ProfileCardType { get; set; }
    public int InstallmentNumber { get; set; }
    public int InstallmentNumberEnd { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal ParentMerchantCommissionRate { get; set; }
    public int BlockedDayNumber { get; set; }
    public bool IsActive { get; set; }   
       
    public Guid PricingProfileId { get; set; }
    public PricingProfile PricingProfile { get; set; }
    
    public virtual List<PricingProfileInstallment> PricingProfileInstallments { get; set; }
}