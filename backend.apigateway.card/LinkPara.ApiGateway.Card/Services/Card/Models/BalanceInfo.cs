namespace LinkPara.ApiGateway.Card.Services.Card.Models;
public class BalanceInfo
{
    public string Type { get; set; }
    public decimal PreviousAmount { get; set; }
    public int CurrencyCode { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal TransactionAmount { get; set; }
}
