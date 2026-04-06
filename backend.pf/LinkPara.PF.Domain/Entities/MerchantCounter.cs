using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantCounter : AuditEntity
{
    public int NumberCounter { get; set; }
}
