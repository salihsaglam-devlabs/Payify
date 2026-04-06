namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;

public class DigitalKycStartRequest
{
    public string PhoneCode { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public long IdentityNo { get; set; }
    public string UserId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Profession { get; set; }
    public int CityId { get; set; }
    public string City { get; set; }
    public int DistrictId { get; set; }
    public string District { get; set; }
    public string CityIso2 { get; set; }
    public string CountryIso2 { get; set; }
    public string Country { get; set; }
    public int? CountryCode { get; set; }
    public string Data { get; set; }
}