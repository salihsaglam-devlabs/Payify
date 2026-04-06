using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities.PhysicalPos;

public class PhysicalPosCurrency : AuditEntity
{
    public Guid PhysicalPosId { get; set; }
    public PhysicalPos PhysicalPos { get; set; }
    public string CurrencyCode { get; set; }
    public Currency Currency { get; set; }
}
