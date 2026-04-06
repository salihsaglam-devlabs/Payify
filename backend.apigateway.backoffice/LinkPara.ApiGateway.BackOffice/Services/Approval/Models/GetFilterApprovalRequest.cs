using LinkPara.ApiGateway.BackOffice.Services.Approval.Models.Enums;
using LinkPara.SharedModels.Pagination;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class GetFilterApprovalRequest : SearchQueryParams
{
    public Guid[] UserRoleIds { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public DateTime? UpdateDateStart { get; set; }
    public DateTime? UpdateDateEnd { get; set; }
    public OperationType? OperationType { get; set; }
    public long? ReferenceId { get; set; }
}
