namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class ConsentedAccountDetailResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public ConsentedAccountDto Result { get; set; }
}