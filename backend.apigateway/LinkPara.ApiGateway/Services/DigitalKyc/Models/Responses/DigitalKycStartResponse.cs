namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;

public class DigitalKycStartResponse
{
    public bool IsSuccessful { get; set; }
    public Result Result { get; set; }
    public string ReferenceId { get; set; }
    public string BackendReference { get; set; }
}
public class Result
{
    public string Code { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public object Message { get; set; }
    public string Info { get; set; }
}