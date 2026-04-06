using LinkPara.HttpProviders.Identity.Models.Enums;

namespace LinkPara.HttpProviders.Identity.Models;

public class PatchUserRequest
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserStatus UserStatus { get; set; }
    public List<Guid> Roles { get; set; }
    public DateTime BirthDate { get; set; }
    public string IdentityNumber { get; set; }
}
