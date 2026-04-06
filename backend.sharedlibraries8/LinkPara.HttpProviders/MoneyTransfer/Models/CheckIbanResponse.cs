namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class CheckIbanResponse
{
    public int BankCode { get; set; }
    public bool IsValidIban { get; set; }
}
