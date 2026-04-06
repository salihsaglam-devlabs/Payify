using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;

public class DisableUserRequest
{
    public Guid UserId { get; set; }
    public UserStatus UserStatus { get; set; }
}