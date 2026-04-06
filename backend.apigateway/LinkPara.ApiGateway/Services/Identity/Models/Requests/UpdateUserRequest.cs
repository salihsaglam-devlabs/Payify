using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class UpdateUserRequest
{
    public string PhoneCode{ get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string IdentityNumber { get; set; }
}

public class UpdateUserServiceRequest : UpdateUserRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
