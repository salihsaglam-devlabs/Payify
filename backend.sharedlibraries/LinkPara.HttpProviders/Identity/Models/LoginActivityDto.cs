
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

namespace LinkPara.HttpProviders.Identity.Models;

public class LoginActivityDto
{
    public Guid UserId { get; set; }
    public string IP { get; set; }
    public DateTime Date { get; set; }
    public string Port { get; set; }
    public LoginResult LoginResult { get; set; }
    public string Channel { get; set; }
}
