using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class DueProfile : AuditEntity, ITrackChange
{
    public string Title { get; set; }
    public DueType DueType { get; set; }
    public decimal Amount { get;set; }
    public int Currency { get; set; }
    public TimeInterval OccurenceInterval { get; set; }
    public bool IsDefault { get; set; }
}