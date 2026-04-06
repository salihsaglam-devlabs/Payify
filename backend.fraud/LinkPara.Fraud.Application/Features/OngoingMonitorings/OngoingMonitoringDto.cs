using LinkPara.Fraud.Application.Commons.Mappings;
using LinkPara.Fraud.Domain.Entities;
using LinkPara.Fraud.Domain.Enums;

namespace LinkPara.Fraud.Application.Features.OngoingMonitorings;

public class OngoingMonitoringDto : IMapFrom<OngoingMonitoring>
{
    public string SearchName { get; set; }
    public SearchType SearchType { get; set; }
    public string ScanId { get; set; }
    public OngoingPeriod Period { get; set; }
    public bool IsOngoingList { get; set; }
    public DateTime CreateDate { get; set; }
}
