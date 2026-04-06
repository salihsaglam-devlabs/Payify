using LinkPara.Fraud.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Fraud.Domain.Entities;

public class OngoingMonitoring : AuditEntity
{
    public string SearchName { get; set; }
    public SearchType SearchType { get; set; }
    public string ScanId { get; set; }
    public OngoingPeriod Period { get; set; }
    public bool IsOngoingList { get; set; }
}
