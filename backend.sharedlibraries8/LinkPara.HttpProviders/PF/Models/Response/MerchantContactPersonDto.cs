using LinkPara.HttpProviders.PF.Models.Enums;
namespace LinkPara.HttpProviders.PF.Models.Response;

public class MerchantContactPersonDto
{
    public Guid Id { get; set; }
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
    public string CreatedBy { get; set; }
}
