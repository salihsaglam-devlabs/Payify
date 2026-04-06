namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class ConsentedAccountActivitiesResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public ConsentedAccountActivityDto Result { get; set; }
}
