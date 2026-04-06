namespace LinkPara.Emoney.Application.Commons.Models.BTransModels;

public class BTransIdentity
{
    public bool IsSucceed { get; set; }
    public bool IsCorporate { get; set; }
    public string TaxNumber { get; set; }
    public string CommercialTitle { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? DocumentType { get; set; }
    public string IdentityNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string NationCountryId { get; set; }
    public string FullAddress { get; set; }
    public string District { get; set; }
    public string PostalCode { get; set; }
    public int? CityId { get; set; }
    public string City { get; set; }
    
}