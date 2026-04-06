
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.Approval.Models;

public class GetCashbackRequestsQuery : SearchQueryParams
{
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public string ActionType { get; set; }
    public string OperationType { get; set; }
    public string[] Statuses { get; set; }
}