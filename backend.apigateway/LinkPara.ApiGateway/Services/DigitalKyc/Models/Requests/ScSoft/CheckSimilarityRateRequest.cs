namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;
public class CheckSimilarityRateRequest
{
    public string SessionId { get; set; }
    public string NfcImage { get; set; }
    public string SelfieImage { get; set; }
    public string IdentityNumber { get; set; }
}
