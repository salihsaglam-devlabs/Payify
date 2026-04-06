namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class ConsentedAccountBalanceDetailResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public ConsentedAccountBalanceDto Result { get; set; }
}