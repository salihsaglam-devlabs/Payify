using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Customers;

public class CustomerDto : IMapFrom<Customer>
{
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
    public MerchantContactPersonDto AuthorizedPerson { get; set; }
}
