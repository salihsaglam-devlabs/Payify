namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class ConsentedAccountActivitiesResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public List<ConsentedAccountActivityDto> Result { get; set; }
}
