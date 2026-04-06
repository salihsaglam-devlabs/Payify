using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Response;

public class OngoingMonitoringResponse
{
    public string SearchName { get; set; }
    public SearchType SearchType { get; set; }
    public string ScanId { get; set; }
    public OngoingPeriod Period { get; set; }
    public bool IsOngoingList { get; set; }
    public DateTime CreateDate { get; set; }
}
