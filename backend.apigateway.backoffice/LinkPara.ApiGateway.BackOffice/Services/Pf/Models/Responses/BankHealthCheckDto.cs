using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class BankHealthCheckDto
{
    public Guid Id { get; set; }
    public Guid AcquireBankId { get; set; }
    public AcquireBankDto AcquireBank { get; set; }
    public DateTime LastCheckDate { get; set; }
    public DateTime AllowedCheckDate { get; set; }
    public int TotalTransactionCount { get; set; }
    public int FailTransactionCount { get; set; }
    public int FailTransactionRate { get; set; }
    public HealthCheckType HealthCheckType { get; set; }
    public bool IsHealthCheckAllowed { get; set; }
}
