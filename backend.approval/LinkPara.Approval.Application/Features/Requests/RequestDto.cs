using LinkPara.Approval.Application.Commons.Mappings;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;
using LinkPara.Approval.Models.Enums;

namespace LinkPara.Approval.Application.Features.Requests;

public class RequestDto : IMapFrom<Request>
{
    public Guid Id { get; set; }
    public long ReferenceId { get; set; }
    public string DisplayName { get; set; }
    public string Resource { get; set; }
    public ApprovalStatus Status { get; set; }
    public ActionType ActionType { get; set; }
    public string Url { get; set; }
    public string QueryParameters { get; set; }
    public string Body { get; set; }
    public Guid MakerRoleId { get; set; }
    public Guid CheckerRoleId { get; set; }
    public Guid SecondCheckerRoleId { get; set; }
    public Guid MakerUserId { get; set; }
    public Guid CheckerUserId { get; set; }
    public Guid SecondCheckerUserId { get; set; }
    public string Reason { get; set; }
    public string CreatedBy { get; set; }
    public string MakerFullName { get; set; }
    public string CheckerFullName { get; set; }
    public string SecondCheckerFullName { get; set; }
    public DateTime UpdateDate { get; set; }
    public RequestOperationType OperationType { get; set; }
    public string MakerDescription { get; set; }
    public string FirstApproverDescription { get; set; }
    public string SecondApproverDescription { get; set; }
    public string ExceptionMessage { get; set; }
    public string ModuleName { get; set; }
}
