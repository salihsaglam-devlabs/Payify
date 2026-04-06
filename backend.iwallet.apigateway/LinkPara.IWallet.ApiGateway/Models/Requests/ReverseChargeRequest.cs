namespace LinkPara.IWallet.ApiGateway.Models.Requests;

public class ReverseChargeRequest
{
    public Guid ProcessGuid { get; set; }
    public decimal ReversedAmount { get; set; }
    public decimal CashBackAmount { get; set; }
}
