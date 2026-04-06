namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class ConsentedAccountBalanceDto
{
    public string HspRef { get; set; }
    public BalanceDetail Bky { get; set; }
}

public class BalanceDetail
{
    public string BkyTtr { get; set; }
    public string BlkTtr { get; set; }
    public string PrBrm { get; set; }
    public DateTime BkyZmnDeger { get; set; }
    public YosCreditAccount KrdHsp { get; set; }
}