using LinkPara.ApiGateway.BackOffice.Services.Approval.Models.Enums;
using LinkPara.Approval.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class GetFilterCashbackApprovalRequest : SearchQueryParams
{
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public ActionType? ActionType { get; set; }
    public RequestOperationType? OperationType { get; set; }
    public ApprovalStatus[]? Statuses { get; set; }
}