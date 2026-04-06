using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Emoney.Domain.Entities;

public class PricingProfileItem : AuditEntity
{
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public WalletType WalletType { get; set; }

    public Guid PricingProfileId { get; set; }
    public PricingProfile PricingProfile { get; set; }
}
