using LinkPara.ApiGateway.Merchant.Commons.Mappings;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;

public class CreateUserRequest
{
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public List<Guid> Roles { get; set; }
}

public class CreateUserWithUserName : CreateUserRequest, IMapFrom<CreateUserRequest>
{
    public string UserName { get; set; }
}
