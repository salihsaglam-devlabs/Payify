using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class SaveBankLimitRequest
{
    public Guid AcquireBankId { get; set; }
    public decimal MonthlyLimitAmount { get; set; }
    public int MarginRatio { get; set; }
    public BankLimitType BankLimitType { get; set; }
    public DateTime LastValidDate { get; set; }
}
