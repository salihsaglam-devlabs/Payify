using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.BankHealthChecks;

public class BankHealthCheckDto : IMapFrom<BankHealthCheck>
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
