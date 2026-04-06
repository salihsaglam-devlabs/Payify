namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.Arksigner;
public class CheckIdentityCardInformationsRequest
{
    public string TransactionId { get; set; }
    public string IdentityNumber { get; set; }
    public string DocumentType { get; set; }
    public string IcaoCode { get; set; }
    public string DocumentSide { get; set; }
    public string AnalysisType { get; set; }
    public string IdentityImageBase64 { get; set; }
}
