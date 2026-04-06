namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class CustomerBalanceDto
{
    public string Authorization { get; set; }
    public string CustomerId { get; set; }
    public BalanceResultDto BalanceResult { get; set; }
}

