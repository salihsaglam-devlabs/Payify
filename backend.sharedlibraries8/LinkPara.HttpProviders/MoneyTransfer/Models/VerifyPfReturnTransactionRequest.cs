namespace LinkPara.HttpProviders.MoneyTransfer.Models;
public class VerifyPfReturnTransactionRequest
{
    public string Description { get; set; }
    public string MerchantNumber { get; set; }
    public string OrderNumber { get; set; }
    public decimal Amount { get; set; }
}
