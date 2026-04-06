namespace LinkPara.SharedModels.BusModels.Commands.Scheduler;

public class CheckBankTransactions
{
    public int BankCode { get; set; }
    public DateTime QueryDate { get; set; }
}
