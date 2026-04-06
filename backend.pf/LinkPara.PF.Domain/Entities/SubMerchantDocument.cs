using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class SubMerchantDocument : AuditEntity
{
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; }
    public Guid SubMerchantId { get; set; }
    public SubMerchant SubMerchant { get; set; }
}
