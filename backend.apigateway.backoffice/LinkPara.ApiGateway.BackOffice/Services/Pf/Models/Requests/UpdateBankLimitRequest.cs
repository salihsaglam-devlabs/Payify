namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateBankLimitRequest
{
    public Guid Id { get; set; }
    public Guid AcquireBankId { get; set; }
    public decimal MonthlyLimitAmount { get; set; }
    public int MarginRatio { get; set; }
    public DateTime LastValidDate { get; set; }
}
