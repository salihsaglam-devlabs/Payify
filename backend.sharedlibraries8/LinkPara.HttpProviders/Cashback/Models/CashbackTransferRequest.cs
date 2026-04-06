namespace LinkPara.HttpProviders.Cashback.Models;

public class CashbackTransferRequest
{
    public Guid EntitlementId { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string RuleDescription { get; set; }
}
