namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class ConsentedAccountsResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public List<ConsentedAccountDto> Result { get; set; }
}