namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class GetTransferBankRequest
{
    public decimal Amount { get; set; }
    public string ReceiverIBAN { get; set; }
    public string CurrencyCode { get; set; }
}
