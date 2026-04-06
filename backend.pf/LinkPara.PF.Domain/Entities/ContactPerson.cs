using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class ContactPerson : AuditEntity, ITrackChange
{
    public ContactPersonType ContactPersonType { get; set; }
    public string IdentityNumber { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string CompanyEmail { get; set; }
    public DateTime BirthDate { get; set; }
    public string CompanyPhoneNumber { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string MobilePhoneNumberSecond { get; set; }
    public Guid ExternalPersonId { get; set; }
}