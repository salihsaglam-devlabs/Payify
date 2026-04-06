using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class SecurityPicture : AuditEntity, ITrackChange
{
    public string Name { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
}
