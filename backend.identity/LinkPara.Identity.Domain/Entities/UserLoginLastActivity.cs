using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class UserLoginLastActivity : AuditEntity  
{
    public DateTime? LastSucceededLogin { get; set; }
    public DateTime? LastLockedLogin { get; set; }
    public DateTime? LastFailedLogin { get; set; }
    public LoginResult LoginResult { get; set; }
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
    public int FailedLoginCount { get; set; } = 0;
    public string Channel { get; set; }

}