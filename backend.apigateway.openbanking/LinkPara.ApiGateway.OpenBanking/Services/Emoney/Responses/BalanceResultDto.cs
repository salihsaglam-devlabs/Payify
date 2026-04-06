namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class BalanceResultDto
{
    public string TotalRecord { get; set; }
    public List<BalanceDto> BalanceList { get; set; }
}

