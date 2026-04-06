using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantLogo : AuditEntity, ITrackChange
{
    public Guid MerchantId { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
}