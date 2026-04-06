namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;
public class SodecCreateSessionResponse
{
    public bool IsSuccessful { get; set; }
    public Result Result { get; set; }
    public string ReferenceId { get; set; }
    public string BackendReference { get; set; }
}
