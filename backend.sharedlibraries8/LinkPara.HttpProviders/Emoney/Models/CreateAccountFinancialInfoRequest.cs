namespace LinkPara.HttpProviders.Emoney.Models;

public class CreateAccountFinancialInfoRequest
{
    public string IncomeSource { get; set; }
    public string IncomeInformation { get; set; }
    public string MonthlyTransactionVolume { get; set; }
    public string MonthlyTransactionCount { get; set; }
    public Guid AccountId { get; set; }
}
