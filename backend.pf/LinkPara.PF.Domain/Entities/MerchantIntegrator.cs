using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantIntegrator : AuditEntity, ITrackChange
{
    public string Name { get; set; }
    public decimal CommissionRate { get; set; }
    public List<Merchant> Merchants { get; set; }
}