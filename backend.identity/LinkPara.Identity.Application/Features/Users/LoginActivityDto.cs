using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

namespace LinkPara.Identity.Application.Features.Users;

public class LoginActivityDto : IMapFrom<LoginActivity>
{
    public Guid UserId { get; set; }
    public string IP { get; set; }
    public DateTime Date { get; set; }
    public string Port { get; set; }
    public LoginResult LoginResult { get; set; }
    public string Channel { get; set; }
}
