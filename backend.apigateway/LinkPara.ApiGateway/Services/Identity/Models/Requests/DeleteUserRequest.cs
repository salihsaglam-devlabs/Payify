using LinkPara.HttpProviders.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class DeleteUserRequest
{
    public UserStatus UserStatus { get; set; }
}
