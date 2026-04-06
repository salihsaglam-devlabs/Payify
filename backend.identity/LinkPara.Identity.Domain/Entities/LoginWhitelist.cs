using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;
public class LoginWhitelist : AuditEntity
{    
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}