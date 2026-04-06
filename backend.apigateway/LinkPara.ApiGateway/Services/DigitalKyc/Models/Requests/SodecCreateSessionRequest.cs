namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;

public class SodecCreateSessionRequest
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
}
