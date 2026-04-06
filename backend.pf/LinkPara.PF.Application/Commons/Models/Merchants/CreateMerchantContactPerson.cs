namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class CreateMerchantContactPerson
{
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