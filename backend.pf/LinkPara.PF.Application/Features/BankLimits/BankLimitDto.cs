using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.BankLimits;

public class BankLimitDto : IMapFrom<BankLimit>
{
    public Guid Id { get; set; }
    public Guid AcquireBankId { get; set; }
    public AcquireBankDto AcquireBank { get; set; }
    public decimal MonthlyLimitAmount { get; set; }
    public int MarginRatio { get; set; }
    public decimal TotalAmount { get; set; }
    public BankLimitType BankLimitType { get; set; }
    public bool IsExpired { get; set; }
    public DateTime LastValidDate { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
