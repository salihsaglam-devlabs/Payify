namespace LinkPara.Emoney.Application.Commons.Models;

public class PayWithWalletLimit
{
    public int PaymentLimit { get; set; }
    public int LoginExpireDayCount { get; set; }
    public int DailyTransactionCount { get; set; }
}
