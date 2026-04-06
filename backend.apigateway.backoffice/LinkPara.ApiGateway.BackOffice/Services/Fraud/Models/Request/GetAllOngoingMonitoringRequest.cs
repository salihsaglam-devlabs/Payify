using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Request;

public class GetAllOngoingMonitoringRequest : SearchQueryParams
{
    public string SearchName { get; set; }
    public SearchType? SearchType { get; set; }
    public OngoingPeriod? OngoingPeriod { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public bool? IsOngoingList { get; set; }
}
