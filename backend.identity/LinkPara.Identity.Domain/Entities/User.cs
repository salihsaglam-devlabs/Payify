using Microsoft.AspNetCore.Identity;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.DomainEvents;

namespace LinkPara.Identity.Domain.Entities;

public class User : IdentityUser<Guid>, IHasDomainEvent, ITrackChange
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string IdentityNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public UserStatus UserStatus { get; set; }
    public string PhoneCode { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string LastModifiedBy { get; set; }
    public string CreatedBy { get; set; }
    public DateTime PasswordModifiedDate { get; set; }
    public List<DomainEvent> DomainEvents { get; set; } = new();
    public RecordStatus RecordStatus { get; set; }
    public Guid? LoginLastActivityId { get; set; }
    public string ExternalCustomerId { get; set; }
    public string ExternalPersonId { get; set; }
    public string AmlReferenceNumber { get; set; }
    public virtual UserLoginLastActivity LoginLastActivity { get; set; }
    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
    public List<Role> Roles { get; set; }
    public List<LoginActivity> LoginActivity { get; set; }
}