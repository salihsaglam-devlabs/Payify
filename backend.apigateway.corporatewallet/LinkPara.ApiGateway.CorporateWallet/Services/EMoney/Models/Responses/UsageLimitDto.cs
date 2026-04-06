namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

public class UsageLimitDto
{
    public bool MaxLimitEnabled { get; set; }
    public decimal DailyMaxAmount { get; set; }
    public int DailyMaxCount { get; set; }
    public decimal MonthlyMaxAmount { get; set; }
    public int MonthlyMaxCount { get; set; }
    public decimal DailyUserAmount { get; set; }
    public int DailyUserCount { get; set; }
    public decimal MonthlyUserAmount { get; set; }
    public int MonthlyUserCount { get; set; }
}