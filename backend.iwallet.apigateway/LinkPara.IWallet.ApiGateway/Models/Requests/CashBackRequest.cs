namespace LinkPara.IWallet.ApiGateway.Models.Requests;

public class CashBackRequest
{
    public string? hash_data { get; set; }
    public string merchant_name { get; set; }
    public List<CashBackTransactionRequest> sales_transactions { get; set; }
    public CashBackTransactionRequest reward_transactions { get; set; }
}
