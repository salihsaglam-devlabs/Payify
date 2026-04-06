namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class IbanLimitDto
{
    public bool MaxLimitEnabled { get; set; }
    public int DailyMaxCount { get; set; }
    public int DailyMaxDistinctCount { get; set; }
    public decimal DailyMaxAmount { get; set; }
    public int MonthlyMaxCount { get; set; }
    public int MonthlyMaxDistinctCount { get; set; }
    public decimal MonthlyMaxAmount { get; set; }
    public int DailyUserCount { get; set; }
    public int DailyUserDistinctCount { get; set; }
    public decimal DailyUserAmount { get; set; }
    public int MonthlyUserCount { get; set; }
    public int MonthlyUserDistinctCount { get; set; }
    public decimal MonthlyUserAmount { get; set; }
}