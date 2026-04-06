namespace LinkPara.PF.Application.Commons.Models.BankHealthChecks;

public class BankHealthCheckInfoModel
{
    public string RangeMinute { get; set; }
    public int MinTransactionCount { get; set; }
    public int FailTransactionRate { get; set; }
}
