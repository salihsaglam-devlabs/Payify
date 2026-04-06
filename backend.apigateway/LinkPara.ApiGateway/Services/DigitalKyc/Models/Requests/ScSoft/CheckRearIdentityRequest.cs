namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;
public class CheckRearIdentityRequest
{
    public string SessionId { get; set; }
    public string Image { get; set; }
    public string IdentityNumber { get; set; }
    public bool IsRead { get; set; }
}
