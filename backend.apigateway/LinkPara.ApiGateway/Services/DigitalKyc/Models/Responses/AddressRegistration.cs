namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;

public class AddressRegistration
{
    public string AddressType { get; set; }
    public string District { get; set; }
    public int DistrictCode { get; set; }
    public string Street { get; set; }
    public int StreetCode { get; set; }
    public int VillageCode { get; set; }
    public string AddressDetail { get; set; }
    public int TownCode { get; set; }
    public string Town { get; set; }
    public string City { get; set; }
    public int CityCode { get; set; }
    public string Country { get; set; }
    public int CountryCode { get; set; }
}
