namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class ConsentedAccountDetailResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public ConsentedAccountDto Result { get; set; }
}