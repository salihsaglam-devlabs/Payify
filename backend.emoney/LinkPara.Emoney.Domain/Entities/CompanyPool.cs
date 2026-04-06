
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class CompanyPool : AuditEntity
{
    public CompanyPoolStatus CompanyPoolStatus { get; set; }
    public CompanyType CompanyType { get; set; }
    public string Title { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string LandPhone { get; set; }
    public string WebSiteUrl { get; set; }
    public string TaxAdministration { get; set; }
    public string TaxNumber { get; set; }
    public string Iban { get; set; }
    public string PostalCode { get; set; }
    public string Address { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
    public int District { get; set; }
    public string DistrictName { get; set; }
    public string AuthorizedPersonIdentityNumber { get; set; }
    public string AuthorizedPersonName { get; set; }
    public string AuthorizedPersonSurname { get; set; }
    public DateTime AuthorizedPersonBirthDate { get; set; }
    public string AuthorizedPersonCompanyPhoneCode { get; set; }
    public string AuthorizedPersonCompanyPhoneNumber { get; set; }
    public string AuthorizedPersonEmail { get; set; }
    public string RejectReason { get; set; }
    public Guid ActionUser { get; set; }
    public DateTime ActionDate { get; set; }
    public CompanyPoolChannel Channel { get; set; }
    public string MersisNumber { get; set; }
    public Guid? AccountId { get; set; }
    public virtual Account Account { get; set; }
}
