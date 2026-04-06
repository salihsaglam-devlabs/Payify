using LinkPara.ApiGateway.BackOffice.Commons.Mappings;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;

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
