using LinkPara.HttpProviders.PF.Models.Enums;

namespace LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;

public class CreateMerchantCustomer
{
    public CompanyType CompanyType { get; set; }
    public string CommercialTitle { get; set; }
    public string TradeRegistrationNumber { get; set; }
    public string TaxAdministration { get; set; }
    public string TaxNumber { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
    public int District { get; set; }
    public string DistrictName { get; set; }
    public string PostalCode { get; set; }
    public string Address { get; set; }
    public Guid ExternalCustomerId { get; set; }
    public CreateMerchantContactPerson AuthorizedPerson { get; set; }
}