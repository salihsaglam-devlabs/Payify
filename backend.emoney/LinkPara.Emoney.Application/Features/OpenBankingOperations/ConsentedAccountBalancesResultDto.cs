namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class ConsentedAccountBalancesResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public List<ConsentedAccountBalanceDto> Result { get; set; }
}