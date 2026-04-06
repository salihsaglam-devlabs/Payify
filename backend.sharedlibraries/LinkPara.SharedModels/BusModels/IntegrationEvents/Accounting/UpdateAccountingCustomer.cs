
namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;

public class UpdateAccountingCustomer
{
    public string CustomerCode { get; set; }
    public string OldCustomerCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string IdentityNumber { get; set; }
    public string Title { get; set; }
    public string City { get; set; }
    public string CityCode { get; set; }
    public string Country { get; set; }
    public string CountryCode { get; set; }
    public string Address { get; set; }
    public string TaxNumber { get; set; }
    public string TaxOffice { get; set; }
    public string TaxOfficeCode { get; set; }
    public string Town { get; set; }
    public string District { get; set; }
}
