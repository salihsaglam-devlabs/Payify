using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantContent : AuditEntity, ITrackChange
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; }
    public MerchantContentSource ContentSource { get; set; }
    public List<MerchantContentVersion> Contents { get; set; }
}