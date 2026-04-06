namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;
public class CheckSpoofRequest
{
    public string SessionId { get; set; }
    public string Image { get; set; }
    public string IdentityNumber { get; set; }
}
