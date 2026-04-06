using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class UserSecurityPicture : AuditEntity, ITrackChange
{
    public Guid UserId { get; set; }
    public Guid SecurityPictureId { get; set; }
    public SecurityPicture SecurityPicture { get; set; }
}
