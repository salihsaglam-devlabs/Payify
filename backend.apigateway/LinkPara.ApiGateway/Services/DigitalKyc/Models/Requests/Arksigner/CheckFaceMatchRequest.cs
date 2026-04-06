namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.Arksigner;
public class CheckFaceMatchRequest
{
    public string TransactionId { get; set; }
    public string FirstFaceBase64 { get; set; }
    public string[] OtherFacesBase64 { get; set; }
    public string IdentityNumber { get; set; }
}
