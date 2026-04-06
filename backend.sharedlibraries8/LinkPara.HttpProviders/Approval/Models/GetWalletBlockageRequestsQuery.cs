
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.Approval.Models;

public class GetWalletBlockageRequestsQuery : SearchQueryParams
{
    public string DisplayName { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public string ActionType { get; set; }
    public string OperationType { get; set; }
    public string[] Statuses { get; set; }
}