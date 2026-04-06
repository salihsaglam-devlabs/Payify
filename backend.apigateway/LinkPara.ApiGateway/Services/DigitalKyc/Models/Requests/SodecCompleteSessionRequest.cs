namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;
public class SodecCompleteSessionRequest
{
    public string Reference { get; set; }
    public int Status { get; set; }
    public string RejectReason { get; set; }
}
