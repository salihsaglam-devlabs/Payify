namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;
public class SodecCompleteSessionResponse
{
    public ResultInfo ResultInfo { get; set; }
}
public class ResultInfo
{
    public bool IsSuccess { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
}