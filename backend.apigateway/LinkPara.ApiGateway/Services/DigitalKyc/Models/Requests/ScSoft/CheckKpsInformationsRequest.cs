namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;
public class CheckKpsInformationsRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string IdentityNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string SessionId { get; set; }
}
