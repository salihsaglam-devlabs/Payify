using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.Identity.Models;

public class UpdateUserRequest
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public List<Guid> Roles { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string AmlReferenceNumber { get; set; }
    public bool? IsBlacklistControl { get; set; }
}

public class UpdateUserWithUserName : UpdateUserRequest
{
    public string UserName { get; set; }
}