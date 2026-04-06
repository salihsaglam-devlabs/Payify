using LinkPara.HttpProviders.CustomerManagement.Models.Enums;

namespace LinkPara.ApiGateway.Services.CustomerManagement.Models.Request;

public class AddCustomerAddressRequest
{
    public Guid CustomerId { get; set; }
    public int CountryId { get; set; }
    public string Country { get; set; }
    public int CityId { get; set; }
    public string City { get; set; }
    public int DistrictId { get; set; }
    public string District { get; set; }
    public string PostalCode { get; set; }
    public string Address { get; set; }
    public string CityIso2 { get; set; }
    public string CountryIso2 { get; set; }
    public AddressType AddressType { get; set; }
}
