using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models;
using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models.Enum;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class CustomerDto
{
    public string Name { get; set; }
    public CompanyType CompanyType { get; set; }
    public string CommercialTitle { get; set; }
    public string TradeRegistrationNumber { get; set; }
    public string TaxAdministration { get; set; }
    public string TaxNumber { get; set; }
    public string MersisNumber { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
    public int District { get; set; }
    public string DistrictName { get; set; }
    public string PostalCode { get; set; }
    public string Address { get; set; }
    public Guid ContactPersonId { get; set; }
    public ContactPersonDto AuthorizedPerson { get; set; }
}
