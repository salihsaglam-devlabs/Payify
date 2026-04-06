using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;
public class LoginActivity : AuditEntity
{
    public Guid UserId { get; set; }
    public string IP { get; set; }
    public DateTime Date { get; set; }
    public string Port { get; set; }
    public LoginResult LoginResult { get; set; }
    public User User { get; set; }
    public string Channel {  get; set; }
}
