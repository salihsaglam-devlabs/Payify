using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class BankLimitDto
{
    public Guid Id { get; set; }
    public Guid AcquireBankId { get; set; }
    public AcquireBankDto AcquireBank { get; set; }
    public decimal MonthlyLimitAmount { get; set; }
    public int MarginRatio { get; set; }
    public decimal TotalAmount { get; set; }
    public BankLimitType BankLimitType { get; set; }
    public DateTime LastValidDate { get; set; }
    public bool IsExpired { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
